using System.Diagnostics;

namespace Crawl.ECS;

public readonly record struct TickInfo(double TargetTps, long ElapsedGameTicks, CancellationToken CancellationToken);

public sealed class TickTimer : IDisposable
{
    private readonly Lock _stateLock = new();
    private readonly Stopwatch _stopwatch;
    private CancellationTokenSource? _cancellationTokenSource;

    private long _elapsedGameTicks;
    private volatile bool _paused;
    private volatile bool _running;
    private Task? _timingTask;

    public TickTimer(double targetTps = 30D)
    {
        if (targetTps <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetTps), "Target TPS must be greater than zero");

        TargetTps = targetTps;
        _stopwatch = new Stopwatch();
    }

    public double TargetTps { get; }

    public double ElapsedSeconds => _stopwatch.ElapsedMilliseconds / 1000.0;
    public long ElapsedGameTicks => Interlocked.Read(ref _elapsedGameTicks);
    public bool IsRunning => _running;
    public bool IsPaused => _paused;

    public void Dispose()
    {
        Stop();
    }

    public event Action<TickInfo>? OnTick;

    public void Start()
    {
        lock (_stateLock)
        {
            if (_running) return;

            _cancellationTokenSource = new CancellationTokenSource();
            _stopwatch.Start();
            _running = true;

            _timingTask = Task.Run(() => TimingLoop(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }
    }

    public void Stop()
    {
        lock (_stateLock)
        {
            if (!_running) return;

            _running = false;
            _cancellationTokenSource?.Cancel();
            _stopwatch.Stop();

            try
            {
                _timingTask?.Wait(TimeSpan.FromSeconds(1));
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
            {
                // Expected when canceling
            }

            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
            _timingTask = null;
        }
    }

    public void Pause()
    {
        if (!_running) return;

        lock (_stateLock)
        {
            if (_paused) return;

            _paused = true;
            _stopwatch.Stop();
        }
    }

    public void Resume()
    {
        if (!_running) return;

        lock (_stateLock)
        {
            if (!_paused) return;

            _paused = false;
            _stopwatch.Start();
        }
    }

    public void Cancel()
    {
        lock (_stateLock)
        {
            _cancellationTokenSource?.Cancel();
        }
    }

    private void TimingLoop(CancellationToken cancellationToken)
    {
        var tickIntervalTicks = (long)(Stopwatch.Frequency / TargetTps);
        var nextTickTime = _stopwatch.ElapsedTicks + tickIntervalTicks;

        try
        {
            while (!cancellationToken.IsCancellationRequested && _running)
            {
                // If paused, just sleep and continue
                if (_paused)
                {
                    Thread.Sleep(10);
                    continue;
                }

                var currentTime = _stopwatch.ElapsedTicks;

                if (currentTime >= nextTickTime)
                {
                    var currentTickCount = Interlocked.Increment(ref _elapsedGameTicks);
                    OnTick?.Invoke(new TickInfo(TargetTps, currentTickCount, cancellationToken));
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