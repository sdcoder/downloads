using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Http
{
    public interface ICookie
    {
        string Path { get; set; }
        bool Secure { get; set; }
        bool HttpOnly { get; set; }
        string Domain { get; set; }
        DateTime Expires { get; set; }
        string Name { get; set; }
        string Value { get; set; }
    }
}