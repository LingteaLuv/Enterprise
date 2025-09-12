using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public static class DataUtility
{
    private static readonly List<string> suffixes = new List<string>
    {
         "", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
         "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"
    };

    /// <summary>
    /// BigInteger 값을 지정된 단위(A, B, C...)에 맞춰 문자열로 변환합니다.
    /// 예: 1234 -> "1.23A", 1234567 -> "1.23B"
    /// </summary>
    /// <param name="number">변환할 BigInteger 숫자</param>
    public static string FormatNumber(BigInteger number)
    {
        if (number == 0) return "0";

        if (number < 1000) return number.ToString();

        int exponent = (int)BigInteger.Log10(number);
        int suffixIndex = exponent / 3;

        // 단위 벗어난 경우 그럴 일은 없을 거 같지만 일단
        if (suffixIndex >= suffixes.Count)
        {
            return "INF";
        }

        var divisor = BigInteger.Pow(1000, suffixIndex);
        double valueToFormat = (double)number / (double)divisor;

        string formattedValue = valueToFormat.ToString("0.##");
        if (formattedValue == "1000" && suffixIndex + 1 < suffixes.Count)
        {
            formattedValue = "1";
            suffixIndex++;
        }

        return formattedValue + suffixes[suffixIndex];
    }

    public static string FormatNumber(float number)
    {
        if (number == 0f) return "0";
        if (number < 1000f) return number.ToString("F0");

        int exponent = (int)Mathf.Floor(Mathf.Log10(number));
        int suffixIndex = exponent / 3;

        if (suffixIndex >= suffixes.Count)
            return "INF";

        float divisor = Mathf.Pow(1000f, suffixIndex);
        float valueToFormat = number / divisor;

        string formattedValue = valueToFormat.ToString("0.##");
        if (formattedValue == "1000" && suffixIndex + 1 < suffixes.Count)
        {
            formattedValue = "1";
            suffixIndex++;
        }

        return formattedValue + suffixes[suffixIndex];
    }
}
