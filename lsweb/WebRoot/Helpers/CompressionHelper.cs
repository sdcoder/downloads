using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Helpers
{
    public static class CompressionHelper
    {
        public static bool SupportsGZipCompression(string acceptEncoding)
        {
            if (string.IsNullOrWhiteSpace(acceptEncoding)) return false;

            return acceptEncoding.Contains("gzip") || acceptEncoding == "*";
        }
    }
}