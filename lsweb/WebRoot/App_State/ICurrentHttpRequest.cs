using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace LightStreamWeb.App_State
{
    public interface ICurrentHttpRequest
    {
        int Port { get; }
        string RootPath { get; }
        NameValueCollection Params { get; }
        NameValueCollection QueryString { get; }
        string UrlReferrer { get; }
        string UrlRequested { get;  }
    }
}
