using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Helpers.WebFormMVC
{
    public class WebFormTempData
    {
        public object this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        private static void Set(string key, object value)
        {
            var td = new WebFormTempDataDictionary();
            SessionStateTempDataProvider tdProvider = new SessionStateTempDataProvider();
            td[key] = value;
            tdProvider.SaveTempData(WebFormController.MockControllerContext(HttpContext.Current), td);
        }

        private static object Get(string key)
        {
            var td = new WebFormTempDataDictionary();
            SessionStateTempDataProvider tdProvider = new SessionStateTempDataProvider();
            var dictionary = tdProvider.LoadTempData(WebFormController.MockControllerContext(HttpContext.Current));
            return dictionary[key];
        }

        public static WebFormTempData Instance
        {
            get
            {
                return new WebFormTempData();
            }
        }


        [Serializable]
        private class WebFormTempDataDictionary : Dictionary<string,object>, IDictionary<string, object>
        {
        }
    }
}