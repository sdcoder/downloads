using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Helpers.Exceptions
{
    public class RedirectException : Exception
    {
        public string RedirectTo { get; private set; }

        public RedirectException(string redirect)
        {
            RedirectTo = redirect;
        }
    }
}