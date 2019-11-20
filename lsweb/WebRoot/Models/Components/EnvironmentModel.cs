using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Components
{
    public class EnvironmentModel
    {
        #region constructors
        private EnvironmentLookup.Environment _environment = EnvironmentLookup.Environment.Production;

        public EnvironmentModel()
        {
            _environment = BusinessConstants.Instance.Environment;
        }

        // for unit testing
        public EnvironmentModel(EnvironmentLookup.Environment e)
        {
            _environment = e;
        }
        #endregion

        public bool IsProduction
        {
            get
            {
                return _environment == EnvironmentLookup.Environment.Production;
            }
        }

        public string IPAddress
        {
            get
            {
                return new LightStreamWeb.App_State.CurrentUser().IPAddress;
            }
        }

        public string TNTId
        {
            get
            {
                return new LightStreamWeb.App_State.CurrentUser().TntId;
            }
        }

        public string EnvironmentName
        {
            get
            {
                if (IsProduction)
                {
                    return string.Empty;
                }

                if (ConfigurationManager.AppSettings["EnvironmentName"] != null)
                {
                    return ConfigurationManager.AppSettings["EnvironmentName"].ToString();
                }
                return _environment.ToString();
            }
        }

        public string VersionNumber
        {
            get
            {
                if (IsProduction)
                    return string.Empty;

                var path = typeof(EnvironmentModel).Assembly.Location;

                return FileVersionInfo.GetVersionInfo(path).FileVersion;
            }
        }
    }
}