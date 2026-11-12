using System.Diagnostics;

namespace CookingMultithreading;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Welcome to our kitchen!!");
        Console.WriteLine("Lesson: AwaitTaskWhenAll");
        Console.WriteLine("Order up! Eggs (3s), Bacon (2s), Toast (1s).");
        Console.WriteLine("Run: Breakfast");

        CookingTimeline timeline = new();
        CookingTrace caller = timeline.CreateTrace("Caller");
        Kitchen kitchen = new(timeline);
        caller.Record("calling kitchen");
        Task breakfast = kitchen.CookBreakfastAsync();
        caller.Record("kitchen call returned");
        await breakfast;
        caller.Record("breakfast ready");
        timeline.Stop();

        timeline.Print();
        Console.WriteLine("Breakfast Order is ready to be served!");
    }
}

internal sealed class Kitchen
{
    private readonly CookingTrace trace;
    private readonly CookingStep eggs;
    private readonly CookingStep bacon;
    private readonly CookingStep toast;

    public Kitchen(CookingTimeline timeline)
    {
        trace = timeline.CreateTrace("Kitchen");
        eggs = new CookingStep(3000, timeline.CreateTrace("Eggs"));
        bacon = new CookingStep(2000, timeline.CreateTrace("Bacon"));
        toast = new CookingStep(1000, timeline.CreateTrace("Toast"));
    }

    public async Task CookBreakfastAsync()
    {
        trace.Record("started");
        trace.Record("calling Eggs.CookAsync");
        Task cookEggs = eggs.CookAsync();
        trace.Record("Eggs task returned");

        trace.Record("calling Bacon.CookAsync");
        Task cookBacon = bacon.CookAsync();
        trace.Record("Bacon task returned");

        trace.Record("calling Toast.CookAsync");
        Task cookToast = toast.CookAsync();
        trace.Record("Toast task returned");

        trace.Record("awaiting all dishes");
        await Task.WhenAll(cookEggs, cookBacon, cookToast);
        trace.Record("WhenAll await returned");
        trace.Record("finished");
    }
}

internal sealed class CookingStep(int milliseconds, CookingTrace trace)
{
    public void Cook()
    {
        trace.Record("started");
        Thread.Sleep(milliseconds);
        trace.Record("finished");
    }

    public async Task CookAsync()
    {
        trace.Record("started");
        await Task.Delay(milliseconds);
        trace.Record("finished");
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
