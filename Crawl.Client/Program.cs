using Crawl.ECS;

var timer = new TickTimer();

timer.OnTick += tick =>
{
    // Check if cancellation was requested
    if (tick.CancellationToken.IsCancellationRequested)
    {
        Console.WriteLine("TickTimer is shutting down...");
        return; // Exit early
    }

    Console.Clear();
    Console.WriteLine($"Tick {tick.ElapsedGameTicks:F0} at {tick.TargetTps:F1} tps");

    // Alternative: Throw if cancellation requested
    // tick.CancellationToken.ThrowIfCancellationRequested();
};

Console.WriteLine("Starting timer... Press any key to stop.");
timer.Start();

// Keep running until user presses a key
Console.ReadKey();
timer.Pause();
Console.ReadLine();
timer.Resume();

Console.ReadLine();
timer.Stop();