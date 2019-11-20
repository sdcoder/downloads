using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages;

namespace LightStreamWeb.Helpers
{
    public static class ViewSectionHelpers
    {
        public static HelperResult RenderSection(this WebPageBase webPage,
              string name, Func<dynamic, HelperResult> defaultContents)
        {
            if (webPage.IsSectionDefined(name))
            {
                return webPage.RenderSection(name);
            }

            return defaultContents(null);
        }
    }
}