using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Helpers
{
    public class RemoveServerHeaderModule : IHttpModule
    {
        public void Dispose()
        {
        }

        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += Context_PreSendRequestHeaders;
        }

        private void Context_PreSendRequestHeaders(object sender, EventArgs e)
        {
            HttpContext.Current.Response.Headers.Remove("Server");
        }
    }
}