﻿using System.Diagnostics;

namespace CookingMultithreading;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to our kitchen!!");

        Kitchen kitchen = new();
        DiningRoom room = new();

        room.OrderBreakfast();
        kitchen.CookBreakfast();
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

    public void CookBreakfast()
    {
        stopwatch.Start();
        CookEggs();
        CookBacon();
        CookToast();
        stopwatch.Stop();
        Console.WriteLine("Time taken: " + stopwatch.Elapsed.ToString(@"m\:ss\.fff"));
        Console.WriteLine("Breakfast Order is ready to be served!");
    }

    void CookEggs()
    {
        Console.WriteLine("Start Cooking Eggs");

        Thread.Sleep(random.Next(3000, 5000));

        Console.WriteLine("Finish Cooking Eggs");
    }

    void CookBacon()
    {
        Console.WriteLine("Start Cooking Bacon");

        Thread.Sleep(random.Next(3000, 5000));

        Console.WriteLine("Finish Cooking Bacon");
    }

    void CookToast()
    {
        Console.WriteLine("Start Cooking Toast");

        Thread.Sleep(random.Next(3000, 5000));

        Console.WriteLine("Finish Cooking Toast");
    }
}
