using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Http
{
    public interface IQueryStringParamService
    {
        string Url { get; }
        IDictionary<string, IList<string>> ParamValues { get; }
        bool Contains(string queryStringParamName);
        string Get(string queryStringParamName);
    }
}