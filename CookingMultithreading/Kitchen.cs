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

    public void CookBreakfast()
    {
        trace.Record("started");
        eggs.Cook();
        bacon.Cook();
        toast.Cook();
        trace.Record("finished");
    }
}
