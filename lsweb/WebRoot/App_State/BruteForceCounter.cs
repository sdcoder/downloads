using FirstAgain.Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.App_State
{
    public class BruteForceCounter
    {
        public const int MAX_REQUESTS = 100;
        private const int MAX_REQUEST_WINDOW = 2;


        protected string GetKeyName(string ipAddress)
        {
            return "BruteForceCount_" + ipAddress;
        }

        public static int Get()
        {
            int count = 0;
            string ipAddress = new CurrentUser().IPAddress;
            string keyName = "BruteForceCount_" + ipAddress;
            if (HttpRuntime.Cache[keyName] != null)
            {
                count = (int)HttpRuntime.Cache[keyName];
            }
            return count;
        }
        public static bool ShouldLockOutIpAddress(ICurrentUser user)
        {
            try
            {
                if (user != null && 
                    user.IPAddress != null 
                    && user.IPAddress != "" &&
                    !user.IPAddress.StartsWith("192.88.0.") && // akamai
                    !user.IPAddress.StartsWith("68.142.133.") && // finicity.com
                    !user.IPAddress.StartsWith("208.93.27.") && // fiserv.com
                    !user.IPAddress.StartsWith("206.108.41") && // mint.com
                    !user.IPAddress.StartsWith("206.225.203")) // mint.com
                {
                    string keyName = "BruteForceCount_" + user.IPAddress;
                    if (HttpRuntime.Cache[keyName] == null)
                    {
                        HttpRuntime.Cache.Insert(keyName, 0, null, DateTime.UtcNow.AddMinutes(MAX_REQUEST_WINDOW), TimeSpan.Zero);
                    }

                    HttpRuntime.Cache[keyName] = (int)HttpRuntime.Cache[keyName] + 1;

                    return ((int)HttpRuntime.Cache[keyName] > MAX_REQUESTS);
                }
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
            }

            return false;
        }
    }
}