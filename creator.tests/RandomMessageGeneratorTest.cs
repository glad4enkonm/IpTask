using creator.util;
using Microsoft.Extensions.Options;
using System.Globalization;
using creator.settings;

namespace creator.tests;

public class RandomMessageGeneratorTests
{
    [Test]
    public void GetRandomIp_GeneratesValidFormat()
    {
        for (int i = 0; i < 100; i++)
        {
            var ip = RandomMessageGenerator.GetRandomIp();
            Assert.That(IsValidIPv4(ip) || IsValidIPv6(ip), Is.True);
        }
    }

    [Test]
    public void GetRandomIp_GeneratesBothIPv4AndIPv6()
    {
        int ipv4Count = 0;
        int ipv6Count = 0;

        for (int i = 0; i < 100; i++)
        {
            var ip = RandomMessageGenerator.GetRandomIp();
            if (IsValidIPv4(ip)) ipv4Count++;
            else if (IsValidIPv6(ip)) ipv6Count++;
        }

        Assert.Multiple(() =>
        {
            Assert.That(ipv4Count, Is.GreaterThan(0), "No IPv4 addresses generated");
            Assert.That(ipv6Count, Is.GreaterThan(0), "No IPv6 addresses generated");
        });
    }

    [Test]
    public void GetRandomId_ReturnsValueWithinConfiguredRange()
    {
        var options = Options.Create(new Generation { UserId = new UserIdSettings { Min = 10, Max = 20 }, Ip = new IpSettings { IPv4Probability = 0.5 } });
        var generator = new RandomMessageGenerator(options);

        for (int i = 0; i < 100; i++)
        {
            var id = generator.GetRandomId();
            Assert.That(id, Is.GreaterThanOrEqualTo(10UL));
            Assert.That(id, Is.LessThanOrEqualTo(20UL));
        }
    }

    [Test]
    public void GetRandomId_WhenMinEqualsMax_ReturnsConstantValue()
    {
        var options = Options.Create(new Generation { UserId = new UserIdSettings { Min = 5, Max = 5 }, Ip = new IpSettings { IPv4Probability = 0.5 } });
        var generator = new RandomMessageGenerator(options);

        var id = generator.GetRandomId();
        Assert.That(id, Is.EqualTo(5UL));
    }

    [Test]
    public void GetRandomMessage_CreatesMessageWithValidProperties()
    {
        var generationOptions = new Generation { UserId = new UserIdSettings { Min = 100, Max = 200 }, Ip = new IpSettings { IPv4Probability = 0.5 } };
        var options = Options.Create(generationOptions);
        var generator = new RandomMessageGenerator(options);

        for (int i = 0; i < 100; i++)
        {
            var message = generator.GetRandomMessage();
            Assert.Multiple(() =>
            {
                Assert.That(message.UserId, Is.GreaterThanOrEqualTo(100UL));
                Assert.That(message.UserId, Is.LessThanOrEqualTo(200UL));
                Assert.That(IsValidIPv4(message.Ip) || IsValidIPv6(message.Ip), Is.True);
            });
        }
    }

    [Test]
    public void Generate_ReturnsValidMessage()
    {
        var generationOptions = new Generation { UserId = new UserIdSettings { Min = 1, Max = 10 }, Ip = new IpSettings { IPv4Probability = 0.5 } };
        var options = Options.Create(generationOptions);
        var generator = new RandomMessageGenerator(options);

        var message = generator.Generate();
        Assert.Multiple(() =>
        {
            Assert.That(message.UserId, Is.InRange(1UL, 10UL));
            Assert.That(IsValidIPv4(message.Ip) || IsValidIPv6(message.Ip), Is.True);
        });
    }

    private static bool IsValidIPv4(string ip)
    {
        var parts = ip.Split('.');
        if (parts.Length != 4) return false;

        // Первый байт: 1-255, остальные: 0-255
        if (!int.TryParse(parts[0], out int part1) || part1 < 1 || part1 > 255)
            return false;

        for (int i = 1; i < 4; i++)
        {
            if (!int.TryParse(parts[i], out int part) || part < 0 || part > 255)
                return false;
        }
        return true;
    }

    private static bool IsValidIPv6(string ip)
    {
        var parts = ip.Split(':');
        if (parts.Length != 8) return false;

        foreach (var part in parts)
        {
            if (part.Length != 4) return false;
            if (!int.TryParse(part, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out _))
                return false;
        }
        return true;
    }
}
