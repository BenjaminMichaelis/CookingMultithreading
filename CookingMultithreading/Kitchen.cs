namespace CookingMultithreading;

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
