using System.Diagnostics;

namespace CookingMultithreading;

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
