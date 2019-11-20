using FirstAgain.Domain.SharedTypes.ContentManagement;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Helpers
{
    public class CMSContentActionResult : ActionResult
    {
        #region helpers for request / response
        private ControllerContext _context;
        private HttpRequestBase Request
        {
            get
            {
                return _context.HttpContext.Request;
            }
        }
        private HttpResponseBase Response
        {
            get
            {
                return _context.HttpContext.Response;
            }
        }
        #endregion

        public override void ExecuteResult(ControllerContext context)
        {
            _context = context;

            string etag = Request.Headers["If-None-Match"];
            if (etag != null)
            {
                int etagId;
                if (Int32.TryParse(etag.Replace("\"", ""), out etagId))
                {
                    Response.AddHeader("ETag", $"\"{etagId}\"");
                    Response.AddHeader("Cache-Control", "public");
                    Response.StatusCode = 304;
                    Response.StatusDescription = "Not Modified";
                    return;
                }
            }

            FileContent content = GetContent();
            if (content == null || content.Content.Length == 0)
            {
                Response.StatusCode = 404;
                Response.StatusDescription = "File not found";
                return;
            }
            
            Response.ContentType = content.MimeType;
            if (!string.IsNullOrEmpty(content.ContentDisposition))
            {
                Response.AddHeader("Content-Disposition", HttpHeaderHelper.SanitizeValue(content.ContentDisposition));
            }
            Response.AddHeader("ETag", $"\"{HttpHeaderHelper.SanitizeValue(content.WebFileContentId.ToString())}\"");
            Response.AddHeader("Cache-Control", "public");
            Response.AddHeader("Content-Length", content.Content.Length.ToString());
            Response.BinaryWrite(content.Content);

            /* // TODO: GZip ahead of time for CMS content.
            if (CMSUtility.ShouldCompressType(content.MimeType) && CompressionHelper.SupportsGZipCompression(Request.Headers.Get("Accept-Encoding")))
            {
                Response.AddHeader("Content-Encoding", "gzip");
                Response.Filter = new GZipStream(Response.Filter, CompressionMode.Compress);
            }
            */
        }


        private void Flush()
        {
            try
            {
                if (Response.IsClientConnected)
                {
                    Response.Flush();
                }
            }
            catch (Exception)
            {
            }
        }
        private FileContent GetContent()
        {
            string id = Request.Params["id"];
            int webFileContentId;

            if (!int.TryParse(id, out webFileContentId))
                return null;

            return new LightStreamWeb.ContentManager().GetFileContent(webFileContentId);
        }


    }
}