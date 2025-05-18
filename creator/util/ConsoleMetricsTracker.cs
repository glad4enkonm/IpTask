using creator.interfaces;
namespace creator.util;

public class ConsoleMetricsTracker: IMetricsTracker
{
    public void TrackBatch(long messagesSent, double elapsedMilliseconds)
    {
        Console.WriteLine($"Отправлено {messagesSent} сообщений за {elapsedMilliseconds:F2} мс");
        if (elapsedMilliseconds > 1000)
        {
            Console.Error.WriteLine($"🚨 Отправлка сообщений заняла больше 1 сек. ({elapsedMilliseconds:F2} мс)");
        }
    }

    public void TrackError(Exception ex)
    {
        Console.Error.WriteLine($"🚨 {DateTime.Now} [ОШИБКА] при отправке сообщения: '{ex}'");
    }

    public void TrackLog(string info)
    {
        Console.WriteLine(info);
    }
}