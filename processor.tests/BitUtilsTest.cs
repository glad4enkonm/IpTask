namespace processor.tests;

using NUnit.Framework;
using System.Net;
using processor.util;

[TestFixture]
public class BitUtilsTest
{
    [Test]
    public void ConvertMaskAndShift_AppliesMaskAndShiftsCorrectly()
    {
        // Arrange
        long value = 0x123456789ABCDEF0;
        string maskHex = "0x0000FF00FF00FF00";
        int shiftBits = 8;

        // Act
        long result = BitUtils.ConvertMaskAndShift(value, maskHex, shiftBits);

        // Assert
        long expectedMask = 0x0000FF00FF00FF00;
        long expectedResult = (value & expectedMask) >> shiftBits;
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [Test]
    public void HexStringToLong_ConvertsValidHexCorrectly()
    {
        // Arrange
        string hex = "0xFF00FF";

        // Act
        long result = BitUtils.HexStringToLong(hex);

        // Assert
        Assert.That(result, Is.EqualTo(0xFF00FF));
    }

    [Test]
    public void HexStringToLong_HandlesNoPrefixCorrectly()
    {
        // Arrange
        string hex = "FF00FF";

        // Act
        long result = BitUtils.HexStringToLong(hex);

        // Assert
        Assert.That(result, Is.EqualTo(0xFF00FF));
    }

    [Test]
    public void HexStringToLong_ThrowsForEmptyInput()
    {
        // Arrange
        string hex = "";

        // Act & Assert
        Assert.That(() => BitUtils.HexStringToLong(hex), Throws.ArgumentException);
    }

    [Test]
    public void HexStringToLong_ThrowsForInvalidHex()
    {
        // Arrange
        string hex = "0xZZZZ";

        // Act & Assert
        Assert.That(() => BitUtils.HexStringToLong(hex), Throws.ArgumentException);
    }

    [Test]
    public void ConvertMaskAndShift_HandlesLargeShiftCorrectly()
    {
        // Arrange
        long value = 0x123456789ABCDEF0;
        string maskHex = "0x0FFFFFFFFFFFFFFF";
        int shiftBits = 60;

        // Act
        long result = BitUtils.ConvertMaskAndShift(value, maskHex, shiftBits);

        // Assert
        Assert.That(result, Is.EqualTo(0)); // Shifting 64 bits right should result in 0
    }

    [Test]
    public void ConvertMaskAndShift_HandlesZeroShiftCorrectly()
    {
        // Arrange
        long value = 0x123456789ABCDEF0;
        string maskHex = "0x0000FF00FF00FF00";
        int shiftBits = 0;

        // Act
        long result = BitUtils.ConvertMaskAndShift(value, maskHex, shiftBits);

        // Assert
        long expectedMask = 0x0000FF00FF00FF00;
        long expectedResult = value & expectedMask;
        Assert.That(result, Is.EqualTo(expectedResult));
    }
}
