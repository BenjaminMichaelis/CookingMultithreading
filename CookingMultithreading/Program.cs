using System.Diagnostics;

namespace CookingMultithreading;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Welcome to our kitchen!!");
        Console.WriteLine("Lesson: SharedStateRace");
        Console.WriteLine("Two cooks each add 1,000 servings to the same counter.");
        Console.WriteLine("Barriers deliberately force the unsafe read-modify-write interleaving.");

        Console.WriteLine("Run: Unsafe counter");
        CookingTimeline unsafeTimeline = new();
        Kitchen unsafeKitchen = new(unsafeTimeline);
        int unsafeServings = unsafeKitchen.CountServings(useAtomicIncrement: false);
        unsafeTimeline.Stop();
        unsafeTimeline.Print();
        Console.WriteLine($"Unsafe servings: {unsafeServings} / {Kitchen.ExpectedServings}");

        Console.WriteLine("Run: Atomic counter");
        CookingTimeline atomicTimeline = new();
        Kitchen atomicKitchen = new(atomicTimeline);
        int atomicServings = atomicKitchen.CountServings(useAtomicIncrement: true);
        atomicTimeline.Stop();
        atomicTimeline.Print();
        Console.WriteLine($"Atomic servings: {atomicServings} / {Kitchen.ExpectedServings}");
        Console.WriteLine("Interlocked.Increment makes the entire update atomic; logging is outside the experiment.");
    }
}

internal sealed class Kitchen(CookingTimeline timeline)
{
    private const int Cooks = 2;
    private const int IncrementsPerCook = 1000;
    public const int ExpectedServings = Cooks * IncrementsPerCook;

    public int CountServings(bool useAtomicIncrement)
    {
        int servings = 0;
        using Barrier phases = new(Cooks);
        CookingTrace coordinator = timeline.CreateTrace("Kitchen");
        Thread[] cooks = new Thread[Cooks];
        for (int cook = 0; cook < cooks.Length; cook++)
        {
            CookingTrace trace = timeline.CreateTrace($"Cook {cook + 1}");
            cooks[cook] = new Thread(() =>
            {
                trace.Record("started");
                for (int serving = 0; serving < IncrementsPerCook; serving++)
                {
                    if (useAtomicIncrement)
                    {
                        phases.SignalAndWait();
                        Interlocked.Increment(ref servings);
                    }
                    else
                    {
                        // Both cooks read before either writes; the second barrier completes both writes.
                        int snapshot = servings;
                        phases.SignalAndWait();
                        servings = snapshot + 1;
                    }

                    phases.SignalAndWait();
                }
                trace.Record("finished");
            });
        }

        coordinator.Record("started");
        foreach (Thread cook in cooks)
        {
            cook.Start();
        }
        foreach (Thread cook in cooks)
        {
            cook.Join();
        }
        coordinator.Record("finished");
        return servings;
    }
}

internal sealed class CookingTimeline
{
    private readonly long startedAt = Stopwatch.GetTimestamp();
    private readonly List<CookingTrace> traces = [];
    private TimeSpan elapsed;

    public CookingTrace CreateTrace(string operation)
    {
        CookingTrace trace = new(operation);
        traces.Add(trace);
        return trace;
    }

    public void Stop()
    {
        elapsed = Stopwatch.GetElapsedTime(startedAt);
    }

    public void Print()
    {
        Console.WriteLine("Elapsed (ms) | Thread | Operation  | Event");
        foreach (CookingEvent entry in traces.SelectMany(trace => trace.Events).OrderBy(entry => entry.Timestamp))
        {
            double milliseconds = Stopwatch.GetElapsedTime(startedAt, entry.Timestamp).TotalMilliseconds;
            Console.WriteLine(FormattableString.Invariant(
                $"{milliseconds,12:F3} | {entry.ThreadId,6} | {entry.Operation,-10} | {entry.Phase}"));
        }

        Console.WriteLine(FormattableString.Invariant($"Cooking elapsed: {elapsed.TotalMilliseconds:F3} ms"));
    }
}

internal sealed class CookingTrace(string operation)
{
    // One logical writer per trace; merge the buffers only after all work has completed.
    private readonly List<CookingEvent> events = [];

    public IReadOnlyList<CookingEvent> Events => events;

    public void Record(string phase)
    {
        events.Add(new CookingEvent(
            Stopwatch.GetTimestamp(), Environment.CurrentManagedThreadId, operation, phase));
    }
}

internal readonly record struct CookingEvent(long Timestamp, int ThreadId, string Operation, string Phase);
