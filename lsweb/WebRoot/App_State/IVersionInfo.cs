using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LightStreamWeb.App_State
{
    public interface IVersionInfo
    {
        string Name { get; set; }
        string Version { get; set; }
    }
}
