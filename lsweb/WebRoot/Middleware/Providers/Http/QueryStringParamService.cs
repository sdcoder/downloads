using ImpromptuInterface;
using LightStreamWeb.Middleware.Services.Http;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Providers.Http
{
    public class QueryStringParamService : IQueryStringParamService
    {
        public string Url => HttpContext.Current.GetOwinContext().Request.Uri.AbsoluteUri;

        public IDictionary<string, IList<string>> ParamValues => this.GetAll();

        private HttpContextBase httpContext => HttpContext.Current.GetOwinContext().Get<HttpContextBase>(typeof(HttpContextBase).FullName);

        public bool Contains(string queryStringParamName)
        {
            if (this.httpContext.Request.QueryString == null)
            {
                return false;
            }

            return this.httpContext.Request.QueryString[queryStringParamName] != null;
        }

        public string Get(string queryStringParamName)
        {
            if (this.Contains(queryStringParamName))
            {
                return this.httpContext.Request.QueryString[queryStringParamName];
            }

            return null;
        }

        private IDictionary<string, IList<string>> GetAll()
        {
            IDictionary<string, IList<string>> dictionary = new Dictionary<string, IList<string>>();

            foreach (var key in this.httpContext.Request.QueryString.AllKeys)
            {
                if (!dictionary.ContainsKey(key))
                {
                    dictionary.Add(key, new List<string> { this.httpContext.Request.QueryString[key] });
                }
                else
                {
                    dictionary[key].Add(this.httpContext.Request.QueryString[key]);
                }
            }

            return dictionary;
        }
    }
}