namespace creator.interfaces;

public interface IMetricsTracker
{
    void TrackBatch(uint messagesSent, double elapsedMilliseconds);
}