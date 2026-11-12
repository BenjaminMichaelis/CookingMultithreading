namespace CookingMultithreading;

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
