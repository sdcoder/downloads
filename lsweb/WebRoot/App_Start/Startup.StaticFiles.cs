using LightStreamWeb.Middleware;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.StaticFiles;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb
{
    public partial class Startup
    {
        private static readonly string[] _cacheControlHeader = new string[]
        {
            "max-age=31536000" // 1 year
        };

        public void ConfigureStaticFiles(IAppBuilder app)
        {
            var fileSystem = new CompressionPhysicalFileSystem(".");

            app.Use(typeof(CompressStaticContentMiddleware), fileSystem);
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileSystem = fileSystem,
                OnPrepareResponse = StaticFile_OnPrepareResponse
            });

            app.UseStageMarker(PipelineStage.MapHandler);
        }

        private void StaticFile_OnPrepareResponse(StaticFileResponseContext context)
        {
            context.OwinContext.Response.Headers.Add("Cache-Control", _cacheControlHeader);
        }
    }
}