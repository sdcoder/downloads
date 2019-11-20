using Owin;
using Microsoft.Owin.StaticFiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb
{
	public partial class Startup
	{
		public void ConfigureStaticContent(IAppBuilder app)
        {
            var options = new StaticFileOptions();

            app.UseStaticFiles(options);
        }
	}
}