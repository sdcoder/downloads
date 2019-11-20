using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.IO;

namespace LightStreamWeb.Middleware
{
    public class CompressStaticContentMiddleware : OwinMiddleware
    {
        private CompressionPhysicalFileSystem _gzipPhysicalFileSystem;

        public CompressStaticContentMiddleware(OwinMiddleware next, CompressionPhysicalFileSystem gzipPhysicalFileSystem)
            : base(next)
        {
            _gzipPhysicalFileSystem = gzipPhysicalFileSystem;
        }

        public override async Task Invoke(IOwinContext context)
        {
            _gzipPhysicalFileSystem.Context = context;

            if (!context.Request.CallCancelled.IsCancellationRequested)
                await Next.Invoke(context);
        }
    }
}