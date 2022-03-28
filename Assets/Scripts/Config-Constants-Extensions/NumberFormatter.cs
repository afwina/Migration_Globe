using System;
using UnityEngine;

public static class NumberFormatter
{
    public static string Format(float num)
    {
        if (num >= 1000)
            return string.Format("{0:#,###0}", num);
        else
            return num.ToString();
    }

    public static string Format(int num)
    {
        if (num >= 1000)
            return string.Format("{0:#,###0}", num);
        else
            return num.ToString();
    }

    public static int Round(int num)
    {
        if (num < 100)
        {
            return num;
        }

        int log10 = Mathf.FloorToInt(Mathf.Log10(num));
        int digitsToZero = log10 - (log10 % 3);
        int roundingFactor = (int) Mathf.Pow(10, digitsToZero);

        return Mathf.RoundToInt(num / roundingFactor ) * roundingFactor;
    }

    public static int Round(uint num)
    {
        if (num < 100)
        {
            return (int) num;
        }

        int log10 = Mathf.FloorToInt(Mathf.Log10(num));
        int digitsToZero = log10 - (log10 % 3);
        int roundingFactor = (int)Mathf.Pow(10, digitsToZero);

        return Mathf.RoundToInt(num / roundingFactor) * roundingFactor;
    }

    public static string RoundPercent(float num)
    {
        return Math.Round(num, 1).ToString("0.0%");
    }
}
