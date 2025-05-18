namespace creator.interfaces;

public interface IMetricsTracker
{
    void TrackBatch(long messagesSent, double elapsedMilliseconds);
    void TrackError(Exception ex);
    void TrackLog(string info);
}