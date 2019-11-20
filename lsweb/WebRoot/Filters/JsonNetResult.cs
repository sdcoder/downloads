using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Filters
{
    public class JsonNetResult : JsonResult
    {
        public static JsonSerializerSettings Settings { get; } = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter>
            {
                new IsoDateTimeConverter(),
                new StringEnumConverter()
            }
        };

        public JsonNetResult() : base() { }

        public override void ExecuteResult(ControllerContext context)
        {
            // verify we have a context
            if (context == null)
                throw new ArgumentNullException("context");

            // get the current http context response
            var response = context.HttpContext.Response;
            // set content type of response
            response.ContentType = !String.IsNullOrEmpty(ContentType) ? ContentType : "application/json";
            // set content encoding of response
            if (ContentEncoding != null)
                response.ContentEncoding = this.ContentEncoding;

            // verify this response has data
            if (this.Data == null)
                return;

            // serialize the object to JSON using JSON.Net
            String JSONText = JsonConvert.SerializeObject(this.Data, Formatting.Indented, Settings);
            // write the response
            response.Write(JSONText);
        }
    }
}