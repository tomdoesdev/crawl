using System.Diagnostics;

namespace Crawl.ECS;

public readonly record struct TickInfo(double TargetTps, long ElapsedGameTicks);

public enum TickTimerState : byte
{
    Running,
    Stopped
}

public sealed class TickTimer : IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Lock _stateLock = new();
    private readonly Stopwatch _stopwatch;

    private long _elapsedGameTicks;


    private volatile TickTimerState _state = TickTimerState.Stopped;
    private Task? _timingTask;


    public TickTimer(double targetTps = 30D, CancellationToken cancellationToken = default)
    {
        if (targetTps <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetTps), "Target TPS must be greater than zero");

        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        TargetTps = targetTps;
        _stopwatch = new Stopwatch();
    }

    public double TargetTps { get; }

    public double ElapsedSeconds => _stopwatch.ElapsedMilliseconds / 1000.0;
    public long ElapsedGameTicks => Interlocked.Read(ref _elapsedGameTicks);
    public bool IsRunning => _state == TickTimerState.Running;

    public bool IsStopped => _state == TickTimerState.Stopped;

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
    }

    public event Action<TickInfo>? OnTick;

    public void Start()
    {
        lock (_stateLock)
        {
            if (IsRunning) return;

            _stopwatch.Start();
            _state = TickTimerState.Running;

            _timingTask ??= Task.Run(() => TimingLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
    }

    public void Stop()
    {
        lock (_stateLock)
        {
            if (IsStopped) return;

            _stopwatch.Stop();
            _state = TickTimerState.Stopped;
        }
    }


    private void TimingLoop(CancellationToken cancellationToken)
    {
        var tickIntervalTicks = (long)(Stopwatch.Frequency / TargetTps);
        var nextTickTime = _stopwatch.ElapsedTicks + tickIntervalTicks;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // If paused, just sleep and continue
                if (IsStopped)
                {
                    Thread.Sleep(10);
                    continue;
                }

                var currentTime = _stopwatch.ElapsedTicks;

                if (currentTime >= nextTickTime)
                {
                    var currentTickCount = Interlocked.Increment(ref _elapsedGameTicks);
                    OnTick?.Invoke(new TickInfo(TargetTps, currentTickCount));
                    nextTickTime += tickIntervalTicks;
                }
                else
                {
                    // Sleep for a small amount to prevent busy-waiting
                    Thread.Sleep(1);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }
    }
}