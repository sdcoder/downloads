using System;
using System.Configuration;
using System.Web;
using System.Web.Configuration;

/// <summary>
/// Summary description for Maintenance
/// </summary>
public static class MaintenanceConfiguration
{
    const string CONFIG_FILE_LOCATION = "~/Maintenance";
    const string MAINTENANCE_MODE_BACKDOOR_KEY_NAME = "lightstream.maintenance.override";

    public static void SetMaintenanceModeBackdoor()
    {

        HttpContext.Current.Response.Cookies.Set(new HttpCookie(MAINTENANCE_MODE_BACKDOOR_KEY_NAME, "true"));
    }

    public static bool IsInMaintenanceMode
    {
        get
        {
            if (HttpContext.Current != null && HttpContext.Current.Request.Cookies[MAINTENANCE_MODE_BACKDOOR_KEY_NAME] != null)
            {
                return false;
            }

            Configuration config = WebConfigurationManager.OpenWebConfiguration(CONFIG_FILE_LOCATION);
            if (bool.Parse(config.AppSettings.Settings["IsInMaintenanceMode"].Value))
            {
                return true;
            }

            if (IsTemporarilyUnavailable)
            {
                return true;
            }

            return MaintenanceConfiguration.ScheduledMaintenanceStartTime.HasValue && ScheduledMaintenanceStartTime.Value < DateTime.Now;
        }

        set
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration(CONFIG_FILE_LOCATION);
            config.AppSettings.Settings["IsInMaintenanceMode"].Value = value.ToString();
            config.Save();
        }
    }

    public static DateTime? ScheduledMaintenanceStartTime
    {
        get
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration(CONFIG_FILE_LOCATION);
            DateTime dt;
            if (DateTime.TryParse(config.AppSettings.Settings["ScheduledMaintenanceStartTime"].Value, out dt))
            {
                return dt;
            }
            // else
            return (DateTime?)null;
        }

        set
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration(CONFIG_FILE_LOCATION);
            config.AppSettings.Settings["ScheduledMaintenanceStartTime"].Value = value.ToString();
            config.Save();
        }
    }

    public static bool IsTemporarilyUnavailable
    {
        get
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration(CONFIG_FILE_LOCATION);

            if (bool.Parse(config.AppSettings.Settings["IsTemporarilyUnavailable"].Value))
            {
                return true;
            }

            return false;

        }
    }


    public static string SignInMaintenanceMessage
    {
        get
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration(CONFIG_FILE_LOCATION);
            return config.AppSettings.Settings["SignInMaintenanceMessage"].Value;
        }

    }

    public static string ScheduledMaintenanceMessage
    {
        get
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration(CONFIG_FILE_LOCATION);
            return config.AppSettings.Settings["ScheduledMaintenanceMessage"].Value;
        }

    }

    public static string TemporarilyUnavailableMessage
    {
        get
        {
            Configuration config = WebConfigurationManager.OpenWebConfiguration(CONFIG_FILE_LOCATION);
            return config.AppSettings.Settings["TemporarilyUnavailableMessage"].Value;
        }

    }
}