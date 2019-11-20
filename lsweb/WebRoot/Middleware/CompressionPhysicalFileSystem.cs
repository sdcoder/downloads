using LightStreamWeb.Helpers;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;

namespace LightStreamWeb.Middleware
{
    public class CompressionPhysicalFileSystem : IFileSystem
    {
        private ThreadLocal<IOwinContext> _context;
        private PhysicalFileSystem _physicalFileSystem;

        public IOwinContext Context
        {
            set { _context.Value = value; }
        }

        public CompressionPhysicalFileSystem(string root)
        {
            _context = new ThreadLocal<IOwinContext>();
            _physicalFileSystem = new PhysicalFileSystem(root);
        }

        public bool TryGetDirectoryContents(string subpath, out IEnumerable<IFileInfo> contents)
        {
            var context = _context.Value;
            bool result = _physicalFileSystem.TryGetDirectoryContents(subpath, out contents);

            string encoding = null;
            string extension = null;
            if (result && DoesClientSupportCompression(context, out encoding, out extension))
            {
                contents = contents.Select(c =>
                {
                    IFileInfo fileInfo = null;
                    if (_physicalFileSystem.TryGetFileInfo($"{c.PhysicalPath}.{extension}", out fileInfo))
                        context.Response.Headers.Set("Content-Encoding", encoding);
                    else
                        fileInfo = c;

                    return fileInfo;
                }).ToArray();
            }

            return result;
        }

        public bool TryGetFileInfo(string subpath, out IFileInfo fileInfo)
        {
            bool result = _physicalFileSystem.TryGetFileInfo(subpath, out fileInfo);

            var context = _context.Value;

            string encoding = null;
            string extension = null;
            if (result && DoesClientSupportCompression(context, out encoding, out extension))
            {
                var temp = fileInfo;
                if (_physicalFileSystem.TryGetFileInfo($"{subpath}.{extension}", out fileInfo))
                    context.Response.Headers.Set("Content-Encoding", encoding);
                else
                    fileInfo = temp;
            }

            return result;
        }

        private bool DoesClientSupportCompression(IOwinContext context, out string encoding, out string extension)
        {
            bool supportsCompression = false;
            encoding = string.Empty;
            extension = string.Empty;

            var acceptEncodingHeader = context.Request.Headers.Get("Accept-Encoding");
            if (CompressionHelper.SupportsGZipCompression(acceptEncodingHeader))
            {
                supportsCompression = true;
                encoding = "gzip";
                extension = "gz";
            }

            return supportsCompression;
        }
    }
}