using contract;
using creator.settings;
using creator.interfaces;
using Microsoft.Extensions.Options;

namespace creator.util;

public class RandomMessageGenerator(IOptions<Generation> options) : IMessageGenerator
{
    private readonly Generation _settings = options.Value;

    /// <summary>
    /// Создайем случайные IPv4 or IPv6 адреса
    /// </summary>
    /// <returns>50% шанс для IPv4, 50% для IPv6</returns>
    static public string GetRandomIp()
    {
        if (_random.Next(2) == 0) // ограничиваемся здесь только исключением адресов тип 0.*
            return $"{_random.Next(1, 256)}.{_random.Next(0, 256)}.{_random.Next(0, 256)}.{_random.Next(0, 256)}";

        var hextets = new string[8];
        for (var i = 0; i < 8; i++)
            hextets[i] = _random.Next(0, 65536).ToString("x4");
        return string.Join(":", hextets);
    }

    public ulong GetRandomId()
        => (ulong)_random.NextInt64(_settings.UserId.Min, _settings.UserId.Max + 1);

    public IpMessage GetRandomMessage() =>
        new() { UserId = GetRandomId(), Ip = GetRandomIp() };

    private static readonly Random _random = Random.Shared;    

    public IpMessage Generate() => GetRandomMessage();
}