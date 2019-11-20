using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Models.Middleware;

namespace LightStreamWeb.Models.PublicSite
{
    public class AccessibilityPageModel : BaseLightstreamPageModel
    {
        public AccessibilityPage cmsContent;

        public AccessibilityPageModel() : this(new ContentManager(), AppSettings.Load().PageDefault.Accessibility)
        {
            
        }
        public AccessibilityPageModel(ContentManager content, LightStreamPageDefault defaults)
        {
            BodyClass = defaults.BodyClass;

            cmsContent = content.Get<AccessibilityPage>();
            if (cmsContent != null)
            {
                if (cmsContent.MetaTagContent != null && cmsContent.MetaTagContent.MetaTagDescription != string.Empty)
                {
                    MetaDescription = cmsContent.MetaTagContent.MetaTagDescription;
                }
                if (cmsContent.MetaTagContent != null && cmsContent.MetaTagContent.MetaTagKeywords != string.Empty)
                {
                    MetaKeywords = cmsContent.MetaTagContent.MetaTagKeywords;
                }
                if (cmsContent.MetaTagContent != null && cmsContent.MetaTagContent.PageTitle != string.Empty)
                {
                    Title = cmsContent.MetaTagContent.PageTitle;
                }
            }
        }


    }
}