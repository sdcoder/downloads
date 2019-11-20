using System.Text.RegularExpressions;

namespace FirstAgain.Web.UI
{
    /// <summary>
    /// Summary description for RegularExpressions
    /// </summary>
    public static class WebRegularExpressions
    {
        public static string StripFromCorrespondenceRegEx = @"<!--BeginStripFromWebView-->(\s|.)*?<!--EndStripFromWebView-->";

        public static bool IsIPAddress(string source)
        {
            var match = Regex.Match(source, @"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})\b");

            return match.Success;
        }
    }

}