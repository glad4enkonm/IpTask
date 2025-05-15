using contract;
namespace creator.util;

public static class Generator
{
    private static readonly Random _random = new();

    const long _minId = 0;
    const long _maxId = 1000000;

    static public string GetRandomIp()
    {
        if (_random.Next(2) == 0)
            return $"{_random.Next(1, 255)}.{_random.Next(0, 255)}.{_random.Next(0, 255)}.{_random.Next(0, 255)}";

        var hextets = new string[8];
        for (var i = 0; i < 8; i++)
            hextets[i] = _random.Next(0, 65535).ToString("x4");
        return string.Join(":", hextets);
    }

    static public ulong GetRandomId()
        => (ulong)_random.NextInt64(_minId, _maxId + 1);

    static public IpMessage GetRandomMessage() =>
        new() { UserId = Generator.GetRandomId(), Ip = Generator.GetRandomIp() };
}