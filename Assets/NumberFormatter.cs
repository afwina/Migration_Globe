using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NumberFormatter
{
    public static string Format(float num)
    {
        return string.Format("{0:#,###0}", num);
    }

    public static string Format(int num)
    {
        return string.Format("{0:#,###0}", num);
    }
}
