using System.Configuration;

namespace LightStreamWeb.Helpers
{
    public static class FeatureSwitch
    {
        public static bool CitizenshipAndNRAQuestionShouldBeDisplayed 
            => GetBoolean("CitizenshipAndNRAQuestionShouldBeDisplayed", true);

        public static bool DisablePrime5NLTRs 
            => GetBoolean("DisablePrime5NLTRs", false);

        public static bool DisablePrime6NLTRs 
            => GetBoolean("DisablePrime6NLTRs", false);

        #region helpers
        private static bool GetBoolean(string name, bool defaultValue)
        {
            if (ConfigurationManager.AppSettings[name] != null)
            {
                bool b = defaultValue;
                if (bool.TryParse(ConfigurationManager.AppSettings[name], out b))
                {
                    return b;
                }
            }
            return defaultValue;
        }
        #endregion
    }
}