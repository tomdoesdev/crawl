using Crawl.ECS;

using var timer = new TickTimer();

timer.OnTick += tick =>
{
    // Check if cancellation was requested

    Console.Clear();
    Console.WriteLine($"Tick {tick.ElapsedGameTicks:F0} at {tick.TargetTps:F1} tps");

    // Alternative: Throw if cancellation requested
    // tick.CancellationToken.ThrowIfCancellationRequested();
};

Console.WriteLine("Starting timer... Press any key to pause.");
timer.Start();
Console.ReadKey();

timer.Stop();
Console.WriteLine("Paused! Press any key to resume");
Console.ReadLine();

timer.Start();
Console.WriteLine("Resumed");

Console.ReadLine();
timer.Stop();
Console.ReadKey();
timer.Stop();
timer.Stop();