using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;


namespace LightStreamWeb.App_State
{

    public static class FailedLoginCounter
    {
        public const int MAX_LOGIN_ATTEMPTS = 3;
        private const int LOCKOUT_TIME_MINUTES = 2;

        private static string GetCacheKeyName()
        {
            string key = "FailedLoginCount_";
            var user = new CurrentUser();
            string uniqueCookie = user.ReadUniqueCookie();
            if (uniqueCookie != null && uniqueCookie != "")
            {
                return key + uniqueCookie;
            }

            if (user.IPAddress != null && user.IPAddress != "")
            {
                key += user.IPAddress;
            }

            key += user.UserAgent.GetHashCode().ToString();

            return key;
        }
        private static int GetFailedLoginCount(string key)
        {
            int CurrentFailedLoginCount = 0;

            if (HttpRuntime.Cache[key] == null)
            {
                InsertUpdateItemInCache(key, "0");
                CurrentFailedLoginCount = 0;
            }
            else
            {
                string TmpCount = Convert.ToString(HttpRuntime.Cache[key]);
                CurrentFailedLoginCount = int.Parse(TmpCount);
            }
            return CurrentFailedLoginCount;
        }

        public static int Get()
        {
            string key = GetCacheKeyName();
            return GetFailedLoginCount(key); 
        }

        public static int Increment()
        {
            var user = new CurrentUser();
            if (user.IPAddress.StartsWith("206.108.41.") ||
                user.IPAddress.StartsWith("::1") ||
                user.IPAddress.StartsWith("192.") ||
                user.IPAddress.StartsWith("184.191.172."))
            {
                return 0;
            }

            string key = GetCacheKeyName();
            int count = GetFailedLoginCount(key);
            count++;
            InsertUpdateItemInCache(key, count.ToString());

            return count;
        }

        public static void Reset()
        {
            string key = GetCacheKeyName();
            RemoveItemFromCache(key);
        }

        private static void RemoveItemFromCache(string key)
        {
            HttpRuntime.Cache.Remove(key);
        }

        private static void InsertUpdateItemInCache(string key, string cachevalue)
        {
            HttpRuntime.Cache.Insert(key, cachevalue, null, DateTime.UtcNow.AddMinutes(LOCKOUT_TIME_MINUTES), TimeSpan.Zero);
        }

    }
}