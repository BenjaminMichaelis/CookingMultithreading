using System.Diagnostics;

namespace CookingMultithreading;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Welcome to our kitchen!!");

        Kitchen kitchen = new();
        DiningRoom room = new();

        room.OrderBreakfast();
        await kitchen.CookBreakfast();
        room.GetAndServeOrder();
    }
}

public class DiningRoom()
{
    public void OrderBreakfast()
    {
        Console.WriteLine("Order up! One order of Breakfast, which is Eggs, Toast, and Bacon");
    }

    public void GetAndServeOrder()
    {
        Console.WriteLine("Check for order from window");
        Thread.Sleep(1000);
        Console.WriteLine("Pick up order from window");
        Thread.Sleep(1000);
        Console.WriteLine("Serve order to table");
    }

    public void CleanRoom()
    {
        Console.WriteLine("Start Clearing Tables");
        Thread.Sleep(1000);
        Console.WriteLine("Finish Clearing Tables");
    }
}

public class Kitchen()
{
    Random random = new();
    private Stopwatch stopwatch = new();

    public async Task CookBreakfast()
    {
        stopwatch.Start();
        Console.WriteLine(nameof(CookEggs) + " Before Task Creation" + " Thread ID:" + Environment.CurrentManagedThreadId);

        Task cookEggs = CookEggs();
        Console.WriteLine(nameof(CookEggs) + " After Task Creation" + " Thread ID:" + Environment.CurrentManagedThreadId);

        Console.WriteLine(nameof(CookBacon) + " Before Task Creation" + " Thread ID:" + Environment.CurrentManagedThreadId);
        Task cookBacon = CookBacon();
        Console.WriteLine(nameof(CookBacon) + " After Task Creation" + " Thread ID:" + Environment.CurrentManagedThreadId);

        Console.WriteLine(nameof(CookToast) + " Before Task Creation" + " Thread ID:" + Environment.CurrentManagedThreadId);
        Task cookToast = CookToast();
        Console.WriteLine(nameof(CookToast) + " After Task Creation" + " Thread ID:" + Environment.CurrentManagedThreadId);
        await cookEggs;
        Console.WriteLine(nameof(CookEggs) + " After Task Wait" + " Thread ID:" + Environment.CurrentManagedThreadId);
        await cookBacon;
        Console.WriteLine(nameof(CookBacon) + " After Task Wait" + " Thread ID:" + Environment.CurrentManagedThreadId);
        await cookToast;
        Console.WriteLine(nameof(CookToast) + " After Task Wait" + " Thread ID:" + Environment.CurrentManagedThreadId);
        stopwatch.Stop();
        Console.WriteLine("Time taken: " + stopwatch.Elapsed.ToString(@"m\:ss\.fff"));
        Console.WriteLine("Breakfast Order is ready to be served!");
    }

    Task CookEggs()
    {
        Console.WriteLine(nameof(CookEggs) + " Before Task Run" + " Thread ID:" + Environment.CurrentManagedThreadId);
        return Task.Run(async () =>
        {
            Console.WriteLine("Start Cooking Eggs" + " Thread ID:" + Environment.CurrentManagedThreadId);

            await Task.Delay(5000);

            Console.WriteLine("Finish Cooking Eggs" + " Thread ID:" + Environment.CurrentManagedThreadId);
        });
    }

    Task CookBacon()
    {
        Console.WriteLine(nameof(CookBacon) + " Before Task Run" + " Thread ID:" + Environment.CurrentManagedThreadId);
        return Task.Run(async () =>
        {
            Console.WriteLine("Start Cooking Bacon" + " Thread ID:" + Environment.CurrentManagedThreadId);

            await Task.Delay(5000);

            Console.WriteLine("Finish Cooking Bacon" + " Thread ID:" + Environment.CurrentManagedThreadId);
        });
    }

    Task CookToast()
    {
        Console.WriteLine(nameof(CookToast) + " Before Task Run" + " Thread ID:" + Environment.CurrentManagedThreadId);

        return Task.Run(async () =>
        {
            Console.WriteLine("Start Cooking Toast" + " Thread ID:" + Environment.CurrentManagedThreadId);

            await Task.Delay(5000);

            Console.WriteLine("Finish Cooking Toast" + " Thread ID:" + Environment.CurrentManagedThreadId);
        });
    }
}
