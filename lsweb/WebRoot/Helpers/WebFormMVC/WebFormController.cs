using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LightStreamWeb.Helpers.WebFormMVC
{
    public class WebFormController : Controller
    {
        public static ControllerContext MockControllerContext(HttpContext context)
        {
            var httpContextWrapper = new HttpContextWrapper(context);

            var routeData = new RouteData();
            routeData.Values.Add("controller", "WebFormController");

            return new ControllerContext(new RequestContext(httpContextWrapper, routeData),
                                                          new WebFormController());

        }
    }
}