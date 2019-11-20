using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;

namespace LightStreamWeb.App_State
{
    public class VersionInfo : IVersionInfo
    {
        public VersionInfo()
        {
            Initialize();
        }

        public string Name { get; set; }

        public string Version { get; set; }

        private void Initialize()
        {
            var assemblyInfo = Assembly.GetExecutingAssembly();

            Name = assemblyInfo.GetName().Name;
            Version = assemblyInfo.GetName().Version.ToString();
        }
    }
}