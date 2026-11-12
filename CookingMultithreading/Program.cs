namespace CookingMultithreading;

internal static class Program
{
    private static void Main()
    {
        Console.WriteLine("Welcome to our kitchen!!");
        Console.WriteLine("Lesson: SharedStateRace");
        Console.WriteLine("Two cooks each add 1,000 servings to the same counter.");
        Console.WriteLine("Barriers deliberately force the unsafe read-modify-write interleaving.");

        Console.WriteLine("Run: Unsafe counter");
        CookingTimeline unsafeTimeline = new();
        Kitchen unsafeKitchen = new(unsafeTimeline);
        int unsafeServings = unsafeKitchen.CountServings(useAtomicIncrement: false);
        unsafeTimeline.Stop();
        unsafeTimeline.Print();
        Console.WriteLine($"Unsafe servings: {unsafeServings} / {Kitchen.ExpectedServings}");

        Console.WriteLine("Run: Atomic counter");
        CookingTimeline atomicTimeline = new();
        Kitchen atomicKitchen = new(atomicTimeline);
        int atomicServings = atomicKitchen.CountServings(useAtomicIncrement: true);
        atomicTimeline.Stop();
        atomicTimeline.Print();
        Console.WriteLine($"Atomic servings: {atomicServings} / {Kitchen.ExpectedServings}");
        Console.WriteLine("Interlocked.Increment makes the entire update atomic; logging is outside the experiment.");
    }
}
