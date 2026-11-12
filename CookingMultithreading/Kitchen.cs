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

        trace.Record("awaiting Eggs");
        await cookEggs;
        trace.Record("Eggs await returned");
        trace.Record("awaiting Bacon");
        await cookBacon;
        trace.Record("Bacon await returned");
        trace.Record("awaiting Toast");
        await cookToast;
        trace.Record("Toast await returned");
        trace.Record("finished");
    }
}
