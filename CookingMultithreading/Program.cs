using System.Diagnostics;

namespace CookingMultithreading;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Welcome to our kitchen!!");
        Console.WriteLine("Lesson: ParallelCooking");
        Console.WriteLine($"Available processors: {Environment.ProcessorCount}");
        Console.WriteLine("Each dish performs 100,000,000 arithmetic iterations; there are no simulated waits.");

        Console.WriteLine("Run: CPU sequential");
        CookingTimeline sequentialTimeline = new();
        Kitchen sequentialKitchen = new(sequentialTimeline);
        long sequential = sequentialKitchen.CookSequentially();
        sequentialTimeline.Stop();
        sequentialTimeline.Print();
        Console.WriteLine($"Sequential checksum: {sequential}");

        Console.WriteLine("Run: CPU parallel");
        CookingTimeline parallelTimeline = new();
        Kitchen parallelKitchen = new(parallelTimeline);
        long parallel = await parallelKitchen.CookInParallelAsync();
        parallelTimeline.Stop();
        parallelTimeline.Print();
        Console.WriteLine($"Parallel checksum: {parallel}");

        if (sequential != parallel)
        {
            throw new InvalidOperationException("The sequential and parallel results differ.");
        }

        Console.WriteLine("Checksums match. Parallel speedup depends on available cores and scheduling.");
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
        eggs = new CookingStep(timeline.CreateTrace("Eggs"));
        bacon = new CookingStep(timeline.CreateTrace("Bacon"));
        toast = new CookingStep(timeline.CreateTrace("Toast"));
    }

    public long CookSequentially()
    {
        trace.Record("started");
        long checksum = eggs.Cook() + bacon.Cook() + toast.Cook();
        trace.Record("finished");
        return checksum;
    }

    public async Task<long> CookInParallelAsync()
    {
        trace.Record("started");
        Task<long> cookEggs = Task.Run(eggs.Cook);
        Task<long> cookBacon = Task.Run(bacon.Cook);
        Task<long> cookToast = Task.Run(toast.Cook);
        long[] checksums = await Task.WhenAll(cookEggs, cookBacon, cookToast);
        trace.Record("finished");
        return checksums.Sum();
    }
}

internal sealed class CookingStep(CookingTrace trace)
{
    private const int Iterations = 100_000_000;

    public long Cook()
    {
        trace.Record("started");
        long checksum = 0;
        for (int ingredient = 0; ingredient < Iterations; ingredient++)
        {
            checksum += ingredient % 97;
        }
        trace.Record("finished");
        return checksum;
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
