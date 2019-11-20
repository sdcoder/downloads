using LightStreamWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Helpers
{
    public static class HeadTagsHelper
    {
        public static MvcHtmlString CanonicalTag(this BaseLightstreamPageModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.Canonical))
            {
                return MvcHtmlString.Empty;
            }

            return MvcHtmlString.Create(string.Format("<link rel=\"canonical\" href=\"{0}\" />", model.Canonical));
        }
    }
}