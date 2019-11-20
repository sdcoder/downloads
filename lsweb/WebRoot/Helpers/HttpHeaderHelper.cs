using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Helpers
{
    public class HttpHeaderHelper
    {
        private class HttpEncoder : System.Web.Util.HttpEncoder
        {
            public void HeaderNameValueEncode_Public(string headerName, string headerValue, out string encodedHeaderName, out string encodededHeaderValue)
            {
                this.HeaderNameValueEncode(headerName, headerValue, out encodedHeaderName, out encodededHeaderValue);
            }
        }

        static readonly HttpEncoder _httpEncoder = new HttpEncoder();
        public static string SanitizeValue(string value)
        {
            string outName = string.Empty;
            string outValue = string.Empty;

            _httpEncoder.HeaderNameValueEncode_Public(string.Empty, value, out outName, out outValue);

            return outValue;
        }
    }
}