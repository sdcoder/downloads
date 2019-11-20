using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Middleware.Services.Affiliates
{
    /// <summary>
    /// Lowercase property names wtih underscores is intended. Object is used to deserialize response from affiliate.
    /// </summary>
    public interface IAffiliate
    {
        string id { get; set; }
        string account_manager_id { get; set; }
        string company { get; set; }
        string address1 { get; set; }
        string address2 { get; set; }
        string city { get; set; }
        string region { get; set; }
        string country { get; set; }
        string other { get; set; }
        string zipcode { get; set; }
        string phone { get; set; }
        string fax { get; set; }
        string website { get; set; }
        string signup_ip { get; set; }
        string date_added { get; set; }
        string status { get; set; }
        string wants_alerts { get; set; }
        string payment_method { get; set; }
        string method_data { get; set; }
        string payment_terms { get; set; }
        string w9_filed { get; set; }
        string referral_id { get; set; }
        string affiliate_tier_id { get; set; }
        string fraud_activity_score { get; set; }
        string fraud_activity_alert_threshold { get; set; }
        string fraud_activity_block_threshold { get; set; }
        string fraud_profile_alert_threshold { get; set; }
        string fraud_profile_block_threshold { get; set; }
        string fraud_risk_tier { get; set; }
        string scrub_offer_pixels { get; set; }
        string ref_id { get; set; }
        string modified { get; set; }
    }
}