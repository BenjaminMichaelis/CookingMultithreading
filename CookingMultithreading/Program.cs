namespace CookingMultithreading;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Welcome to our kitchen!!");
        Console.WriteLine("Lesson: AsyncCooking");
        Console.WriteLine("Order up! Eggs (3s), Bacon (2s), Toast (1s).");
        Console.WriteLine("Run: Breakfast");

        CookingTimeline timeline = new();
        CookingTrace caller = timeline.CreateTrace("Caller");
        Kitchen kitchen = new(timeline);
        caller.Record("calling kitchen");
        kitchen.CookBreakfast();
        caller.Record("kitchen call returned");
        caller.Record("breakfast ready");
        timeline.Stop();

        timeline.Print();
        Console.WriteLine("Breakfast Order is ready to be served!");
    }
}
