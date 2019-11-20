using FirstAgain.Common.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Helpers
{
    public static class ThirdPartySettingsHelper
    {
        public static string GetTransUnionPublicKey()
        {
            return ThirdPartyRegistrar.GetThirdPartySettings("TransUnionFPE").GetSetting("PublicKey");
        }
    }
}