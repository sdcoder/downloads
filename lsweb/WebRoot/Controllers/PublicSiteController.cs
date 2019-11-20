using FirstAgain.Common.Logging;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Helpers;
using LightStreamWeb.Helpers.Exceptions;
using LightStreamWeb.Models.Middleware;
using LightStreamWeb.Models.PublicSite;
using Ninject;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.SessionState;

namespace LightStreamWeb.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class PublicSiteController : BaseController
    {
        private IAppSettings Settings;

        [Inject]
        public PublicSiteController(ICurrentUser user, IAppSettings settings) : base(user)
        {
            Settings = settings;
        }

        [OutputCache(Duration = 60, VaryByParam = "*")]
        public ActionResult RedesignPage(MarketingRedirectParameters p)
        {
            string redirect;
            string enableOldWebRedirectsString = WebConfigurationManager.AppSettings["EnableOldWebRedirects"];
            bool enableOldWebRedirects = false;
            if (Boolean.TryParse(enableOldWebRedirectsString, out enableOldWebRedirects) &&
                enableOldWebRedirects &&
                new MarketingRedirectHelper().CheckForMarketingRedirect(p, Request.Url.Query, out redirect))
            {
                return Redirect(redirect);
            }

            var model = new RedesignPageModel(new ContentManager(), Settings.PageDefault.HomePage)
            {
                FirstAgainCodeTrackingId = WebUser.FirstAgainCodeTrackingId,
                NavClass = "bluetint",
                NavAutoLabel = "Auto"
            };

            return View(model);
        }

        //
        // GET: /faq
        [OutputCache(Duration = 60, VaryByParam = "*")]
        public ActionResult FAQ(string id)
        {
            if (string.IsNullOrEmpty(id) || id == "552") // CMS preview
            {
                return View(new FAQModel());
            }

            // handle legacy URLs
            if (id.Equals("LandingPage.aspx", StringComparison.InvariantCultureIgnoreCase))
            {
                int webContentId;
                if (int.TryParse(Request.QueryString["id"], out webContentId))
                {
                    string queryParameterValue = FAQLandingPageModel.GetQueryParameterValue(webContentId);

                    var url = "/faq/" + queryParameterValue;

                    if (Url.IsLocalUrl(url))
                    {
                        return new RedirectResult(url, true);
                    }

                    return View(new FAQModel());

                }
            }

            // else, it's a landing page FAQ
            return View("FAQ_LandingPage", new FAQLandingPageModel()
            {
                LandingPageUrl = id
            });
        }

        //
        // GET: /LandingPage/Custom
        public ActionResult CustomLandingPage(string urlRewritePath, bool mbox = false)
        {
            try
            {
                return View("LandingPageTemplates/CustomTemplate", new CustomLandingPageModel());
            }
            catch (RedirectException rex)
            {
                return Redirect(VirtualPathUtility.ToAbsolute(rex.RedirectTo));
            }
        }

        public ActionResult UrlRewrite()
        {
            var helper = new WebRedirectHelper();

            // These parameters were set by the route constraint.
            var webRedirectGroup = RouteData.Values[nameof(WebRedirectGroup)] as WebRedirectGroup;
            var marketingRedirectParameters = RouteData.Values[nameof(MarketingRedirectParameters)] as MarketingRedirectParameters;

            var redirectString = helper.GetWebRedirectUrl(
                webRedirectGroup,
                marketingRedirectParameters
                );

            if (string.IsNullOrWhiteSpace(redirectString))
            {
                return new HttpNotFoundResult(); // 404
            }

            helper.SetCookies(
                webRedirectGroup,
                marketingRedirectParameters
                );

            return RedirectPermanent(redirectString); // 301
        }

        private ComponentUrlSetting[] FetchComponentUrlSettings()
        {
            var componentUrlSettings = new List<ComponentUrlSetting>();
            Newtonsoft.Json.Linq.JObject componentUrls = AppSettings.Load().PageDefault.ComponentUrls as Newtonsoft.Json.Linq.JObject;
            if (componentUrls != null)
            {
                foreach (Newtonsoft.Json.Linq.JProperty url in componentUrls.Children())
                {

                    TemplateComponentType component;
                    if (Enum.TryParse<TemplateComponentType>(url.Name, out component))
                    {
                        var componentSetting = new ComponentUrlSetting(component, url.Name, url.Value.ToString());
                        componentUrlSettings.Add(componentSetting);
                    }
                }
            }
            return componentUrlSettings.ToArray();
        }

        //
        // GET: /LandingPage
        [OutputCache(Duration = 60, VaryByParam = "*")]
        public ActionResult LandingPage(string urlRewritePath, bool mbox = false)
        {
            try
            {
                var model = new LandingPageModel(urlRewritePath);

                model.CheckAndSetFactCookie(Request.QueryString["fact"]);
                model.ComponentUrlSettings.AddRange(FetchComponentUrlSettings());

                if (model.IsRedirectTestBlankPageEnabled && !mbox)
                {
                    return View("~/Views/PublicSite/BlankPage.cshtml");
                }

                return View($"~/Views/PublicSite/LandingPageTemplates/{model.TemplateName}.cshtml", model);
            }
            catch (RedirectException rex)
            {
                return Redirect(VirtualPathUtility.ToAbsolute(rex.RedirectTo));
            }
        }

        //
        // GET: /press-kit (etc...)
        public ActionResult PRMedia(string tab)
        {
            var model = new PRMediaPagesModel()
            {
                OpenTab = tab,
                SingleTabMode = (tab == "press-releases" || tab == "in-the-news")
            };

            return View(model);
        }

        //
        // GET: /customer-testimonials
        public ActionResult CustomerTestimonials()
        {
            var model = new AboutPagesModel(new ContentManager(), Settings.PageDefault.About)
            {
                OpenTab = "customer-testimonials"
            };
            model.SingleTabMode = true;
            model.AppendBodyClass("single-tab");

            return View(model);
        }

        //
        // GET: /press-kit (etc...)
        public ActionResult About(string tab)
        {
            var model = AboutPagesModelFactory.Create(tab, Settings.PageDefault.About);
            model.OpenTab = tab;
            if (tab == "licensing" || tab == "customer-testimonials")
            {
                model.SingleTabMode = true;
                model.AppendBodyClass("single-tab");
            }

            ViewBag.BannerImageAlt = model.Banner?.Image?.Alt;

            return View(model);
        }

        //
        // GET: /rate-beat
        public ActionResult RateBeat()
        {
            var model = new RateMatchPageModel();
            model.Title = "LightStream Loans Rate Beat Program";
            model.AppendBodyClass("rate-match");
            return View("RateMatch", model);
        }

        private bool IsValidPartial(string content)
        {
            // basic security checks
            if (content.Length > 255 || content.Contains("/") || content.Contains(@"\") || content.StartsWith("."))
            {
                return false;
            }
            string name = "~/Views/PublicSite/Partials/" + content + ".cshtml";
            ViewEngineResult result = ViewEngines.Engines.FindView(ControllerContext, name, null);
            return (result.View != null);
        }

        //
        // GET: /who-we-are (etc...) - typically accordian or modal content
        public ActionResult CMSPartial(string content)
        {
            if (content == "cms" || !IsValidPartial(content))
            {
                return new HttpNotFoundResult();
            }
            return PartialView("~/Views/PublicSite/Partials/" + content + ".cshtml", new ContentManager());
        }

        //
        // GET: /partial/who-we-are (etc...)
        public ActionResult Partial(string content)
        {
            if (content == "cms" || !IsValidPartial(content))
            {
                return new HttpNotFoundResult();
            }
            return PartialView("~/Views/PublicSite/Partials/" + content + ".cshtml");
        }

        //
        // GET: /
        [OutputCache(Duration = 60, VaryByParam = "*")]
        public ActionResult PrivacySecurity(string tab)
        {
            return View(new PrivacySecurityModel(new ContentManager(), Settings.PageDefault.PrivacySecurity)
            {
                OpenTab = tab
            });
        }

        //
        // GET: /
        public ActionResult PrivacySecurityPopup()
        {
            return PartialView("~/Views/PublicSite/Modals/Privacy.cshtml", new ContentManager());
        }
        //
        // GET: /modals/contact-us
        [HttpGet]
        public ActionResult ContactUsPopup()
        {
            return PartialView("~/Views/PublicSite/Modals/ContactUs.cshtml", new ContactUsModel());
        }
        //
        // GET: /modals/customer-testimonials
        [HttpGet]
        public ActionResult CustomerTestimonialsPopup()
        {
            return PartialView("~/Views/PublicSite/Modals/CustomerTestimonials.cshtml", new ContentManager());
        }
        //
        // POST: /modals/contact-us
        [HttpPost]
        public ActionResult ContactUsPopup(ContactUsModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return new JsonNetResult()
                    {
                        Data = new
                        {
                            Success = false,
                            ErrorMessage = ModelState.ToString()
                        },

                    };
                }

                model.Send();
                return new JsonNetResult()
                {
                    Data = new { Success = true }
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new JsonNetResult()
                {
                    Data = new { Success = true, ErrorMessage = ex.ToString() }
                };

            }
        }

        //
        // GET: /ContactUs
        [HttpGet]
        public ActionResult ContactUs()
        {
            return View(new ContactUsModel()
            {
                Canonical = "https://www.lightstream.com/contact-us"
            });
        }

        //
        // POST: /ContactUs
        [HttpPost]
        public ActionResult ContactUs(ContactUsModel model)
        {
            if (ModelState.IsValid)
            {
                model.Send();
            }

            return View("ContactUsSubmitted", model);
        }

        //
        // GET: /SiteMap
        [OutputCache(Duration = 600)] // 10 minutes
        public ActionResult SiteMap()
        {
            return View(new SiteMapPageModel()
            {
                Canonical = "https://www.lightstream.com/site-map"
            });
        }

        //
        // GET: /HappyBirthday
        //[OutputCache(Duration = 600)] // 10 minutes
        public ActionResult HappyBirthday()
        {
            return View(new BasePublicSiteModel()
            {
                Title = "Happy Birthday!"
            });
        }
        //
        // GET: /PlantATreeCard
        //[OutputCache(Duration = 600)] // 10 minutes
        public ActionResult PlantATreeCard()
        {
            return View(new BasePublicSiteModel()
            {
                Title = "We Plant a Tree with Every Loan!"
            });
        }
        //
        // GET: /HolidayeCard
        //[OutputCache(Duration = 600)] // 10 minutes
        public ActionResult HolidayeCard()
        {
            return View(new BasePublicSiteModel()
            {
                Title = "A Special Holiday Message"
            });
        }
        //
        // GET: /accessibility
        [HttpGet]
        [Route("accessibility")]
        public ActionResult Accessibility()
        {
            return View(new AccessibilityPageModel());
        }



    }
}
