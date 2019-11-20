using HttpCache.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace LightStreamWeb.Models.Webhooks
{
    public class KenticoWebhookModel
    {
        [MaxLength(1000)] // for Veracode
        public string Event { get; set; }
        public dynamic Payload { get; set; }
    }
}