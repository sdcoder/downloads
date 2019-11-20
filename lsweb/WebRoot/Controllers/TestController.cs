using FirstAgain.Common.Logging;
using FirstAgain.Common.Web;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.ServerState;
using LightStreamWeb.App_State;
using LightStreamWeb.Models.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using LightStreamWeb.Filters;
using LightStream.Service.Cache.Client;
using System.Web.Security.AntiXss;
using LightStreamWeb.Models;

namespace LightStreamWeb.Controllers
{
    public class TestController : Controller
    {

        [NonProdOnly]
        public ActionResult XSSTest(XSSTestModel model)
        {
            return View(model);
        }
        public ActionResult XSSTest2()
        {
            return View("XSSTest", new XSSTestModel());
        }

        [HttpPost]
        public ActionResult TestCoBrandPageModel(BaseCoBrandPageModel model)
        {
            return new EmptyResult();
        }
        [HttpPost]
        public ActionResult TestBaseLightstreamPageModel(BaseLightstreamPageModel model)
        {
            return new EmptyResult();
        }


        public ActionResult PingWebContext()
        {
            try
            {
                DomainServiceWebContextOperations.PingWebContext();
                return new ContentResult() { Content = "OK" };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new ContentResult() { Content = AntiXssEncoder.HtmlEncode(ex.Message, true) };
            }
        }
        public ActionResult Index()
        {
            return new HttpNotFoundResult();
        }

        public ActionResult RateCalculatorIFrame(int? fact, string purposeOfLoan)
        {
            ViewBag.purposeOfLoan = purposeOfLoan;
            ViewBag.fact = fact;
            var marketingData = DomainServiceLoanApplicationOperations.GetCachedMarketingData();

            List<Tuple<string, int>> facts = new List<Tuple<string, int>>();
            var suntrustMarketingOrg = marketingData.LkpMarketingOrganization.Where(m => m.Caption.Contains("SunTrust"));
            foreach (var org in suntrustMarketingOrg)
            {
                foreach (var fDetail in marketingData.FirstAgainCodeTrackDetail.Where(f => f.MarketingOrganizationId == org.MarketingOrganizationId))
                {
                    facts.Add(new Tuple<string, int>(org.Caption, fDetail.FirstAgainCodeTrackingId));
                }

            }

            return View(facts);
        }

        [NonProdOnly]
        public ActionResult SubscribeFormPreview()
        {
            return View();
        }



        //
        // GET: /Test/referral
        [NonProdOnly]
        public ActionResult referral(int id)
        {
            return new ContentResult()
            {
                Content = "<a href='/apply/continue?id=" + WebSecurityUtility.Scramble(id).ToString() + "'>click here</a>"
            };
        }

        //
        // Get: /Test/Boom
        public ActionResult Boom()
        {
            // to test error module, duh
            throw new Exception();
        }

        [NonProdOnly]
        public ActionResult CMSDemo()
        {
            return View(new LightStreamWeb.Models.PublicSite.PublicSiteWithBannerModel());
        }
        [NonProdOnly]
        public ActionResult StyleGuide()
        {
            return View(new LightStreamWeb.Models.PublicSite.PublicSiteWithBannerModel());
        }

        // /Test/MaintenanceBackdoor
        // -> redirected from /Test/MaintenanceBackdoor.aspx
        public ActionResult MaintenanceBackdoor()
        {
            MaintenanceConfiguration.SetMaintenanceModeBackdoor();
            return Redirect("/customer-sign-in/");
        }

        // GET: /Test/Cache
        [NonProdOnly]
        public ActionResult Cache()
        {
            return View(FirstAgain.Common.Caching.MachineCache.Entries);
        }

        // GET: /Test/RefreshCache
        public ActionResult RefreshCache(string id)
        {
            if (BusinessConstants.Instance.Environment == EnvironmentLookup.Environment.Production)
            {
                return new HttpUnauthorizedResult();
            }
            var cacheEntry = FirstAgain.Common.Caching.MachineCache.Entries.FirstOrDefault(a => a.Key == id);
            if (cacheEntry == null)
            {
                return new ContentResult() { Content = "No cache entry found for " + AntiXssEncoder.HtmlEncode(id, true) };
            }

            cacheEntry.ForceRefresh();

            return Redirect("/Test/Cache");
        }

        // GET: /Test/RefreshAll
        [NonProdOnly]
        public ActionResult RefreshAll()
        {
            foreach (var cacheEntry in FirstAgain.Common.Caching.MachineCache.Entries)
            {
                cacheEntry.ForceRefresh();
            }

            return Redirect("/Test/Cache");
        }

        // GET: /Test/ViewCacheEntry
        [NonProdOnly]
        public ActionResult ViewCacheEntry(string id)
        {
            var cacheEntry = FirstAgain.Common.Caching.MachineCache.Entries.FirstOrDefault(a => a.Key == id);
            if (cacheEntry == null)
            {
                return new ContentResult() { Content = "No cache entry found for " + AntiXssEncoder.HtmlEncode(id, true) };
            }

            string xml;
            try
            {
                // because it's more pretty than the DataContractSerializer
                xml = FirstAgain.Common.Xml.XmlUtility.SerializeToString(cacheEntry.Value, cacheEntry.CacheEntryType);
            }
            catch (Exception)
            {
                xml = FirstAgain.Common.Xml.XmlUtility.DataContractSerializerToString(cacheEntry.Value, cacheEntry.CacheEntryType);
            }

            if (xml.Length > 100000)
            {
                xml = xml.Substring(0, 100000) + "\r\n\r\n ******************** TRUNCATED ********************* ";
            }
            var model = new ViewCacheEntryModel()
            {
                Title = id,
                XML = xml
            };

            return View(model);
        }

        public ActionResult AMLBypass()
        {
            new CurrentUser().WebRedirectId = LoanApplicationDataSet.KnownFACTs.LIGHTSTREAM_AML_BYPASS;
            return new JsonResult()
            {
                Data = "ok",
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        public ActionResult MockIp(string NewIPAddress)
        {
            if (BusinessConstants.Instance.Environment == EnvironmentLookup.Environment.Production)
            {
                return new HttpNotFoundResult();
            }

            return new JsonResult()
            {
                Data = new LightStreamWeb.App_State.CurrentUser().MockIPAddress(NewIPAddress),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
        public ActionResult MockSessionId(int? MockSessionId)
        {
            if (BusinessConstants.Instance.Environment == EnvironmentLookup.Environment.Production)
            {
                return new HttpNotFoundResult();
            }

            SessionUtility.ActiveApplicationId = MockSessionId;
            return new EmptyResult();
        }

        [NonProdOnly]
        public ActionResult NullSession()
        {
            SessionUtility.Set("test", null);
            return new EmptyResult();
        }

        [NonProdOnly]
        public ActionResult IP()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<h2>Fix</h2>");
            sb.AppendFormat("CurrentUser().IPAddress: {0} <br/>", new LightStreamWeb.App_State.CurrentUser().IPAddress);

            sb.Append("<h2>Select Request.ServerVariables</h2>");
            sb.AppendFormat("REMOTE_ADDR: {0} <br/>", Request.ServerVariables["REMOTE_ADDR"]);
            sb.AppendFormat("X-Forwarded-For: {0} <br/>", Request.ServerVariables["X-Forwarded-For"]);
            sb.AppendFormat("Via: {0} <br/>", Request.ServerVariables["Via"]);
            sb.AppendFormat("HTTP_X_FORWARDED_FOR: {0} <br/>", Request.ServerVariables["HTTP_X_FORWARDED_FOR"]);
            sb.AppendFormat("Request.UserHostAddress: {0} <br/>", Request.UserHostAddress);
            sb.Append("<hr>");

            sb.Append("<h2>Select  Request.Headers</h2>");
            sb.AppendFormat("REMOTE_ADDR: {0} <br/>", Request.Headers["REMOTE_ADDR"]);
            sb.AppendFormat("X-Forwarded-For: {0} <br/>", Request.Headers["X-Forwarded-For"]);
            sb.AppendFormat("Via: {0} <br/>", Request.Headers["Via"]);
            sb.AppendFormat("HTTP_X_FORWARDED_FOR: {0} <br/>", Request.Headers["HTTP_X_FORWARDED_FOR"]);
            sb.AppendFormat("Request.UserHostAddress: {0} <br/>", Request.UserHostAddress);
            sb.Append("<hr>");

            sb.Append("<h2>ALL Request.ServerVariables</h2>");
            foreach (var k in Request.ServerVariables.AllKeys)
            {
                sb.AppendFormat("{0}: {1} <br/>", k, Request.ServerVariables[k]);
            }

            sb.Append("<hr>");
            sb.Append("<h2>ALL Request.Headers</h2>");
            foreach (var k in Request.Headers.AllKeys)
            {
                sb.AppendFormat("{0}: {1} <br/>", k, Request.Headers[k]);
            }

            return new ContentResult()
            {
                Content = sb.ToString()
            };
        }

        [NonProdOnly]
        public ActionResult IFrame()
        {
            return View();
        }

        [NonProdOnly]
        public ActionResult Video()
        {
            return View();
        }

        [NonProdOnly]
        public ActionResult ENV()
        {
            StringBuilder showMeTheENV = new StringBuilder("<pre>");
            foreach (string k in Environment.GetEnvironmentVariables().Keys)
            {
                showMeTheENV.AppendLine(string.Format("{0}: {1} <br>", k, Environment.GetEnvironmentVariable(k).ToString()));
            }

            return new ContentResult() { Content = showMeTheENV.ToString() };
        }
        [NonProdOnly]
        public ActionResult Headers()
        {
            StringBuilder showMeTheHeaders = new StringBuilder("<pre>");
            foreach (string k in Request.Headers.AllKeys)
            {
                showMeTheHeaders.AppendLine(string.Format("{0}: {1} <br>", k, Request.Headers[k]));
            }

            return new ContentResult() { Content = showMeTheHeaders.ToString() };
        }

        public ActionResult GetCustomerServiceInbox()
        {
            try
            {
                var inbox = new BusinessInformationClient(new Endpoints().CacheServiceUrl).GetCustomerServiceInbox();
                if (inbox == null)
                {
                    return new ContentResult()
                    {
                        Content = "inbox is null"
                    };
                }
                return new ContentResult()
                {
                    Content = inbox.EmailAddress
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                throw;
            }
        }


        [NonProdOnly]
        public ActionResult GetDisplayRate()
        {
            try
            {
                var rates = new RateInformationClient(new Endpoints().CacheServiceUrl).GetDisplayRate("HomeImprovement", null);
                if (rates == null)
                {
                    return new ContentResult()
                    {
                        Content = "rates is null"
                    };
                }
                return new ContentResult()
                {
                    Content = rates.PurposeOfLoanCaption
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                throw;
            }
        }


    }
}
