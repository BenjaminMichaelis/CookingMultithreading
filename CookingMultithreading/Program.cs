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
