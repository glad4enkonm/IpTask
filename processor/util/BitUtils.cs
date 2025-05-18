namespace processor.util;

public static class BitUtils
{
    
    public static long ConvertMaskAndShift(long value, string maskHex, int shiftBits)
    {        
        long mask = HexStringToLong(maskHex);
        return (value & mask) >> shiftBits;
    }


    public static long HexStringToLong(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            throw new ArgumentException("Шестнадцатиричная строка для преобразования не должна быть пустой.");

        // Убираем лишнее
        hex = hex.Trim().ToLowerInvariant();
        if (hex.StartsWith("0x"))
            hex = hex[2..];        

        // Разбираем
        if (!long.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out long result))        
            throw new ArgumentException($"Не смогли разобрать шестнадцатиричную строку: {hex}");

        return result;
    }
}