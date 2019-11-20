using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.SignIn
{
    public class InvalidSecurityInfoModel : BaseLightstreamPageModel
    {
        public InvalidSecurityInfoModel()
        {
            BodyClass = "sign-in";
        }

        public string PageTitle { get; set; }
    }
}