using FirstAgain.Common.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace LightStreamWeb.Models.Middleware
{
    public class AppSettings : IAppSettings
    {
        private const string AppSettingsFile = "appsettings.json";

        public int CustomerDataVerificationClientTimeoutMilliseconds { get; set; }       

        public Pagedefault PageDefault { get; set; }

        public string CdnBaseUrl { get; set; }

        public static AppSettings Load(string filename = "")
        {
            var path = filename;
            if (string.IsNullOrEmpty(path))
            {
                var directory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
                path = Path.Combine(directory, AppSettingsFile);
            }
            if (File.Exists(path))
            {
                string content = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<AppSettings>(content);
            }
            else
            {
                LightStreamLogger.WriteError("App settings file not found at " + path);
            }
            return new AppSettings()
            {
                CdnBaseUrl = "https://www.suntrust.com/content/dam/lightstream/us/en/content/images",
                CustomerDataVerificationClientTimeoutMilliseconds = 4000,
                PageDefault = new Pagedefault()
                {
                    HomePage = new LightStreamPageDefault(),
                    About = new LightStreamPageDefault()
                }
            };
        }
    }

    public class LightStreamPageDefault
    {
        public string Banner { get; set; }
        public string BannerAlt { get; set; }
        public string BenefitStatement { get; set; }
        public string BodyClass { get; set; }
        public string Heading { get; set; }
        public string PageTitle { get; set; }
        public string SubHeading { get; set; }
    }
    
    public class Pagedefault
    {
        public LightStreamPageDefault About { get; set; }
        public LightStreamPageDefault ContactUs { get; set; }
        public LightStreamPageDefault HomePage { get; set; }
        public LightStreamPageDefault PrivacySecurity { get; set; }
        public LightStreamPageDefault RateBeat { get; set; }
        public LightStreamPageDefault Rates { get; set; }
        public object ComponentUrls { get; set; }
        public LightStreamPageDefault Accessibility { get; set; }
    }
}