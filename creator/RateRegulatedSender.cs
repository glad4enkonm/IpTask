using creator.interfaces;
using creator.settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace creator;
public class RateRegulatedSender : IRateRegulatedSender
{
    private readonly System.Timers.Timer _timer;
    private readonly IMessageBatchSender _sender;
    private readonly IMetricsTracker _metricsTracker;
    private readonly uint _batchSize;
    private readonly long _rate;

    public RateRegulatedSender(IMessageBatchSender sender, IMetricsTracker metricsTracker, IOptions<Generation> settings)
    {
        _sender = sender;
        _metricsTracker = metricsTracker;

        _batchSize = settings.Value.BatchSize;
        _rate = settings.Value.Rate;
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += async (sender, e) => await OnTimerElapsed();
        _metricsTracker.TrackLog($"Начинаем отправку на скорости {_rate} ...");
    }

    private async Task OnTimerElapsed()
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var tasks = new List<Task>();
            long remaining = _rate;

            while (remaining > 0)
            {
                uint batchSize = (uint)Math.Min(_batchSize, remaining); // результат не может быть больше batchSize => uint
                tasks.Add(_sender.SendBatch(batchSize));
                remaining -= batchSize;
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            stopwatch.Stop();
            _metricsTracker.TrackBatch(_rate, stopwatch.Elapsed.TotalMilliseconds);
        }
    }

    public void Start() => _timer.Start();
    public void Stop() => _timer.Stop();
}