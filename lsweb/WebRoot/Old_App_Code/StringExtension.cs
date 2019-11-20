using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

public static class StringExtension
{
    public static string ToTitleCase(this string str)
    {
        var cultureInfo = Thread.CurrentThread.CurrentCulture;
        return cultureInfo.TextInfo.ToTitleCase(str.ToLower());
    }

    public static string ToScrubbed(this string str)
    {
        return Regex.Replace(str, @"([\s_\-]+)", string.Empty);
    }

}