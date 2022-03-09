using System.Collections;
using System.Collections.Generic;
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
}
