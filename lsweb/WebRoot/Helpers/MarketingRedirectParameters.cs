using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Helpers
{
    // These parameter names should match what are in the QueryString sent by partners.
    // Do not change spelling, casing, or anything else 
    public class MarketingRedirectParameters
    {
        public string fact { get; set; }
        public string fair { get; set; }
        public string subId { get; set; }
        public string AID { get; set; }
        public string BRLId { get; set; }
        public string GSLID { get; set; }
        public string ClickId { get; set; }
        public string irmp { get; set; }
        public string ef_id { get; set; }
        public string irpid { get; set; }
        public string isredirect { get; set; }

        public Dictionary<string, object> OtherValues { get; } = new Dictionary<string, object>();
    }
}