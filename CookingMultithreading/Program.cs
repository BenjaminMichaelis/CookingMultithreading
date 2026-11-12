namespace CookingMultithreading;

internal static class Program
{
    private static async Task Main()
    {
        Console.WriteLine("Welcome to our kitchen!!");
        Console.WriteLine("Lesson: ParallelCooking");
        Console.WriteLine($"Available processors: {Environment.ProcessorCount}");
        Console.WriteLine("Each dish performs 100,000,000 arithmetic iterations; there are no simulated waits.");

        Console.WriteLine("Run: CPU sequential");
        CookingTimeline sequentialTimeline = new();
        Kitchen sequentialKitchen = new(sequentialTimeline);
        long sequential = sequentialKitchen.CookSequentially();
        sequentialTimeline.Stop();
        sequentialTimeline.Print();
        Console.WriteLine($"Sequential checksum: {sequential}");

        Console.WriteLine("Run: CPU parallel");
        CookingTimeline parallelTimeline = new();
        Kitchen parallelKitchen = new(parallelTimeline);
        long parallel = await parallelKitchen.CookInParallelAsync();
        parallelTimeline.Stop();
        parallelTimeline.Print();
        Console.WriteLine($"Parallel checksum: {parallel}");

        if (sequential != parallel)
        {
            throw new InvalidOperationException("The sequential and parallel results differ.");
        }

        Console.WriteLine("Checksums match. Parallel speedup depends on available cores and scheduling.");
        Console.WriteLine("Breakfast Order is ready to be served!");
    }
}
