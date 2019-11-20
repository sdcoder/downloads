using FirstAgain.Domain.SharedTypes.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.PublicSite
{
    public class ComponentUrlSetting
    {
        public ComponentUrlSetting() { }
        public ComponentUrlSetting(TemplateComponentType templateComponentType, string enumerationName, string url)
        {
            this.TemplateComponentType = templateComponentType;
            this.EnumerationName = enumerationName;
            this.Url = url;
        }
        public TemplateComponentType TemplateComponentType;
        public string EnumerationName;
        public string Url;
    }
}