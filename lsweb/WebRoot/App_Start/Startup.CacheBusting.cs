using FirstAgain.Common.Logging;

using LightStream.Service.Cache.CacheBusting;
using LightStreamWeb.Middleware;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;

namespace LightStreamWeb
{
    public partial class Startup
    {
        private static Dictionary<string, string> fileHashMap;

        private ICacheBustService cachingService;

        public void ConfigureCacheBusting()
        {
            cachingService = new CacheBustService(HttpContext.Current.Server.MapPath("."),
                                                  ConfigurationManager.AppSettings["Components:FileHashLocation"],
            new CacheBustOptions
            {
                FailedFileHashGeneration = FailedHashGeneration,
                FileHashMapGenerated = FileHashMapGenerated
            });

            cachingService.Initialize();

            fileHashMap = cachingService.FileHashMap; 
        }

        public static string GetFileHash(string filePath)
        {
            return fileHashMap[filePath.Replace("/", "\\")];
        }

        private void FailedHashGeneration(string fileHashLocations, Dictionary<string, string> fileHashMap, Exception ex)
        {

            var errorMessage = new
            {
                Message = "Failed to generate file hashes",
                FileHashMap = fileHashMap,
                Locations = fileHashLocations,
            };

            LightStreamLogger.WriteError(ex);
        }

        private void FileHashMapGenerated(string fileHashLocations, Dictionary<string, string> fileHashMap)
        {
            LightStreamLogger.WriteInfo("File hash map generated. FileHashMap: {FileHashMap}, Locations: {FileHashLocations}",
                fileHashMap, fileHashLocations);
        }
    }
}
