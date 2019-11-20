using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace LightStreamWeb.Helpers.WebFormMVC
{
    public static class WebFormHelper
    {
        public static void RenderPartial(string partialName, object model)
        {
            //get a wrapper for the legacy WebForm context
            var httpCtx = new HttpContextWrapper(System.Web.HttpContext.Current);

            //create a mock route that points to the empty controller
            var rt = new RouteData();
            rt.Values.Add("controller", "WebFormController");

            //create a controller context for the route and http context
            var ctx = new ControllerContext(
                new RequestContext(httpCtx, rt), new WebFormController());

            //find the partial view using the viewengine
            var view = ViewEngines.Engines.FindPartialView(ctx, partialName).View;
            if (view == null)
            {
                throw new Exception("View " + partialName + " could not be found");
            }
            var viewContext = new ViewContext(ctx, view, new ViewDataDictionary { Model = model }, new TempDataDictionary(), httpCtx.Response.Output);

            //render the partial view
            view.Render(viewContext, System.Web.HttpContext.Current.Response.Output);
        }
    }
}