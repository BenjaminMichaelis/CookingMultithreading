namespace CookingMultithreading;

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
