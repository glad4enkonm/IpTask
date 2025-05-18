using System.Net;
using System.Net.Sockets;
using System.Text;

namespace processor.util;

public static class IpAddressFormatter
{
    public static string ToSearchableString(IPAddress ipAddress)
    {
        ArgumentNullException.ThrowIfNull(ipAddress);

        if (ipAddress.AddressFamily == AddressFamily.InterNetwork) // IPv4
            return ipAddress.ToString();
        else if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6) // IPv6
        {
            // Получаем полный, не сжатый IPv6 адрес
            // 8 частей по 4 шестнадцатиричных символа, дополненный нулями по надобности
            byte[] bytes = ipAddress.GetAddressBytes();
            if (bytes.Length != 16) // не должно случаться, но проверим
                throw new ArgumentException("Неправильная IPv6 длина в байтах.", nameof(ipAddress));

            var sb = new StringBuilder(39); // Максимальная длина для IPv6: 8 * 4 символов + 7 разделителей
            for (int i = 0; i < 16; i += 2)
            {
                if (i > 0)
                    sb.Append(':');
                sb.AppendFormat("{0:x2}{1:x2}", bytes[i], bytes[i + 1]);
            }
            return sb.ToString().ToLowerInvariant(); // приводим к нижнему регистру
        }
        else
        {
            // Этот случай не должен случаться с INET types, но проверим
            return ipAddress.ToString();
        }
    }
}