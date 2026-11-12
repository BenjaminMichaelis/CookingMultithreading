namespace CookingMultithreading;

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
