using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Helpers
{
    public static class BytesHelper
    {
        public static MvcHtmlString FormatBytes(this long sizeInBytes)
        {
            const int scale = 1024;
            string[] orders = new string[] {"GB", "MB", "KB", "Bytes" };
            long max = (long)Math.Pow(scale, orders.Length - 1);
            foreach (var order in orders)
            {
                if (sizeInBytes > max)
                {
                    return new MvcHtmlString(string.Format("{0:##.##} {1}", decimal.Divide(sizeInBytes, max), order));
                }

                max /= scale;
            }

            return new MvcHtmlString("0 bytes");
        }
    }
}