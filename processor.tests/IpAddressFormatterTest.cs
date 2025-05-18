namespace processor.tests;

using NUnit.Framework;
using System.Net;
using processor.util;

[TestFixture]
public class IpAddressFormatterTest
{
    [Test]
    public void ToSearchableString_NullIpAddress_ThrowsArgumentNullException()
    {
        Assert.That(() => IpAddressFormatter.ToSearchableString(null), Throws.ArgumentNullException);
    }

    [Test]
    public void ToSearchableString_ValidIPv4Address_ReturnsString()
    {
        var ipAddress = IPAddress.Parse("192.168.1.1");
        var result = IpAddressFormatter.ToSearchableString(ipAddress);
        Assert.That(result, Is.EqualTo("192.168.1.1"));
    }

    [Test]
    public void ToSearchableString_ValidIPv6Address_ReturnsUncompressedLowercase()
    {
        var ipv6Address = IPAddress.Parse("2001:0db8:85a3::8a2e:0370:7334");
        var result = IpAddressFormatter.ToSearchableString(ipv6Address);
        var expected = "2001:0db8:85a3:0000:0000:8a2e:0370:7334";
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToSearchableString_AnotherIPv6Address_ReturnsCorrectFormat()
    {
        var ipv6Address = IPAddress.Parse("fe80:0000:0000:0000:0202:b3ff:fe1e:8329");
        var result = IpAddressFormatter.ToSearchableString(ipv6Address);
        var expected = "fe80:0000:0000:0000:0202:b3ff:fe1e:8329";
        Assert.That(result, Is.EqualTo(expected));
    }

    [Test]
    public void ToSearchableString_CannonicalIPv6Address_ReturnsCorrectFormat()
    {
        var ipv6Address = IPAddress.Parse("fe80::0202:b3ff:fe1e:8329");
        var result = IpAddressFormatter.ToSearchableString(ipv6Address);
        var expected = "fe80:0000:0000:0000:0202:b3ff:fe1e:8329";
        Assert.That(result, Is.EqualTo(expected));
    }

}