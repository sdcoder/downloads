using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;
using FirstAgain.Common.Extensions;
using Microsoft.Ajax.Utilities;
using System.Text.RegularExpressions;

namespace LightStreamWeb.Helpers
{
    public class TrustedSiteHelper
    {
        /// <summary>
        /// Dictionary with urls allowed framing permissions as key, value is the sites they're allowed to frame.  
        /// A null value indicates no limitation on permissions
        /// </summary>
        private static Dictionary<string, List<string>> _urlsWithFramingPermission =
            new Dictionary<string, List<string>>
            {
                    // Testing
                    { "http://dappcluster:755/car-loans-and-financing", 
                        new List<string>
                        {
                             // TODO: Change to "kbb-auto" and possibly include query parameters for FACT once marketing is complete with their page
                            "https://dweb03/kbb-used-car",
                        }
                    },
                    // Testing
                    { "http://qappcluster:755/car-loans-and-financing", 
                        new List<string>
                        {
                             // TODO: Change to "kbb-auto" and possibly include query parameters for FACT once marketing is complete with their page
                            "https://test.lightstream.com/kbb-used-car",
                        }
                    },
                    // Testing - for Iframe tool
                    { "http://localhost:55897/Test/IFrame", null },
                    { "https://dweb03/Test/IFrame", null },
                    { "https://test.lightstream.com/Test/IFrame", null },
                    { "https://test2.lightstream.com/Test/IFrame", null },

                    // Internal
                    { "http://dappcluster:703/ContentManagement", null },
                    { "http://qappcluster:703/ContentManagement", null },
                    { "http://q2appcluster:703/ContentManagement", null },
                    { "http://sappcluster:706/ContentManagement", null },
                    { "http://pappcluster:706/ContentManagement", null },
                    { "http://localhost:27290/ContentManagement", null },
                    
                    // Suntrust DEV environment
                    { "http://10.7.*", null },
                    { "https://10.7.*", null },
                    { "http://10.7.218.8/Static/IpLightstream.html", null },
                    { "https://10.7.218.8/Static/IpLightstream.html", null },
                    { "http://10.7.215.8/Static/IpLightstream.html", null },
                    { "https://10.7.215.8/Static/IpLightstream.html", null },
                    { "https://stcom-dev1.suntrust.com", null },
                    { "https://cms-dev1.suntrust.com", null },
                    { "https://stcom-itca.suntrust.com", null },
                    { "https://cms-itca.suntrust.com", null },
                    { "https://cms-prdr.suntrust.com", null },
                    { "https://stcom-prdr.suntrust.com", null },

                    // Partner
                    {
                        // This is to allow any request from KBB.com from any subdomain and any page
                        @"^(http(s)?)\:\/\/((www)|([a-z0-9]+)\.)?(kbb.com)\b([-a-zA-Z0-9@:% _\+.~#?&//=]*)",  
                        new List<string>
                        {
                            "https://www.lightstream.com/kbb-auto",
                            "https://test.lightstream.com/kbb-auto",
                            "https://test2.lightstream.com/kbb-auto",
                            // The below items are just for testing via KBB's page through localhost
                            //"https://dweb03/kbb-auto",
                            //"http://localhost:55897/kbb-auto"
                        }
                    },
                    // Suntrust
                    {
                        // This is to allow any request from suntrust.com from any subdomain and any page
                        @"^(http(s)?)\:\/\/((www)|([a-z0-9\-]+)\.)?(suntrust.com)\b([-a-zA-Z0-9@:% _\+.~#?&//=]*)",
                        new List<string>
                        {
                            "https://www.lightstream.com/rates/widget",
                            "https://test.lightstream.com/rates/widget",
                            "https://test2.lightstream.com/rates/widget",
                        }
                    }
            };

        public static bool CanIframe(string urlReferrer, string urlRequest)
        {
            if (string.IsNullOrWhiteSpace(urlReferrer) || string.IsNullOrWhiteSpace(urlRequest)) return false;

            string permissionKey = null;
            foreach (var url in _urlsWithFramingPermission.Keys)
            {
                Regex eval = new Regex(url,RegexOptions.IgnoreCase);
                if (eval.IsMatch(urlReferrer))
                {
                    permissionKey = url;
                    break;
                }
            }            
            if (!string.IsNullOrEmpty(permissionKey))
            {
                List<string> urlsCanBeFramed;
                if (_urlsWithFramingPermission.TryGetValue(permissionKey, out urlsCanBeFramed))
                {
                    return urlsCanBeFramed.IsNull() || urlsCanBeFramed.Any(urlRequest.StartsWith);
                }
            }

            return false;
        }
    }
}