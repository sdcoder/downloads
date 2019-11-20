using System.Security.Policy;
using LightStreamWeb.Helpers;
using LightStreamWeb.Helpers.WebFormMVC;
using System;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Configuration;
using LightStreamWeb.RouteConstraints;
using System.Web.Http;
using LightStreamWeb.App_Start;
using FirstAgain.Domain.Common;

namespace LightStreamWeb
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;
            routes.MapMvcAttributeRoutes();

            // Start addition to disable this code path.
            string enableNewWebRedirectsString = WebConfigurationManager.AppSettings["EnableNewWebRedirects"];
            bool enableNewWebRedirects = false;

            if (Boolean.TryParse(enableNewWebRedirectsString, out enableNewWebRedirects) && enableNewWebRedirects)
            {
                routes.MapRoute(
                    name: "WebRedirect_VanityUrl",
                    url: "{*permalink}",
                    defaults: new { controller = "PublicSite", action = "UrlRewrite" },
                    constraints: new { permalink = new WebRedirectRouteConstraint() }
                );

            }
            // End addition to disable this code path.

            routes.MapRoute(
                name: "StaticLookups",
                url: "Lookups/StaticLookups/{action}",
                defaults: new { controller = "Lookups", action = "StaticLookups" }
            );

            routes.MapRoute(
                name: "HomePageRedesign",
                url: "redesign",
                defaults: new { controller = "PublicSite", action = "RedesignPage" });

            // generate a signature from our font
            routes.MapRoute(
                name: "Signature",
                url: "Signature/{action}",
                defaults: new { controller = "Signature", action = "Generate" }
            );

            // JSON methods for Improved Property Address methods
            routes.MapRoute(
                name: "SubjectPropertyAddress",
                url: "SubjectPropertyAddress/{action}",
                defaults: new { controller = "SubjectPropertyAddress" }
            );
            // JSON methods for VIN methods
            routes.MapRoute(
                name: "VehicleInformation",
                url: "VehicleInformation/{action}/{vin}",
                defaults: new { controller = "VehicleInformation", action = "Validate", vin = UrlParameter.Optional }
            );
            // JSON methods for banking info
            routes.MapRoute(
                name: "BankingInformation",
                url: "services/BankingInformation/{action}",
                defaults: new { controller = "BankingInformation", action = "Index", id = UrlParameter.Optional }
            );
            // Other JSON methods 
            routes.MapRoute(
                name: "StateLookup",
                url: "Services/StateLookup",
                defaults: new { controller = "LocationInformation", action = "StateLookup", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "StateTaxLookup",
                url: "Services/StateTaxLookup",
                defaults: new { controller = "LocationInformation", action = "StateTaxLookup", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "NonApplicantSpouseData",
                url: "Services/LoanAcceptance/{action}",
                defaults: new { controller = "LoanAcceptance", action = "NonApplicantSpouseData", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "JSONToServerSessionSupport",
                url: "Services/session/{action}",
                defaults: new { controller = "Session", action = "Get", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "ValidateBranchCostCenter",
                url: "Services/ValidateBranchCostCenter/{id}",
                defaults: new { controller = "SuntrustIntegration", action = "ValidateBranchCostCenter", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "ValidateEmployeeId",
                url: "Services/ValidateEmployeeId/{id}",
                defaults: new { controller = "SuntrustIntegration", action = "ValidateEmployeeId", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "ValidateOfficerId",
                url: "Services/ValidateOfficerId/{id}",
                defaults: new { controller = "SuntrustIntegration", action = "ValidateOfficerId", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "AutosuggestOccupation",
                url: "Services/Autosuggest/Occupation",
                defaults: new { controller = "Autosuggest", action = "Occupation", id = UrlParameter.Optional }
            );

            // Lending Tree, other tests
            routes.MapRoute(
                name: "Test",
                url: "Test/{action}/{id}",
                defaults: new { controller = "Test", action = "Index", id = UrlParameter.Optional }
            );
            // ping
            routes.MapRoute(
                name: "Ping",
                url: "Ping/{action}/{id}",
                defaults: new { controller = "Ping", action = "Index", id = UrlParameter.Optional }
            );
            // "about us", "privacy", and "FAQ" friendly URLs
            SetupPublicSitePartialRoutes(routes);

            // modals
            routes.MapRoute(
                name: "PrivacySecurityPopup",
                url: "modals/privacy",
                defaults: new { controller = "PublicSite", action = "PrivacySecurityPopup" }
            );
            routes.MapRoute(
                name: "ContactUsPopup",
                url: "modals/contact-us",
                defaults: new { controller = "PublicSite", action = "ContactUsPopup" }
            );
            routes.MapRoute(
                name: "CustomerTestimonialsModal",
                url: "modals/customer-testimonials",
                defaults: new { controller = "PublicSite", action = "CustomerTestimonialsPopup" }
            );
            routes.MapRoute(
                name: "account-services-contact-us",
                url: "modals/account-services-contact-us",
                defaults: new { controller = "AccountServices", action = "ContactUsPopup" }
            );

            // ENotices
            routes.MapRoute(
                name: "eNotices",
                url: "enotices/{action}/{id}",
                defaults: new { controller = "ENotices", action = "Modal", id = UrlParameter.Optional }
            );


            // account lock
            routes.MapRoute(
                name: "r-account-lock-AccountLock",
                url: "SignInLocked/{action}/{id}",
                defaults: new { controller = "SignIn", action = "AccountLock", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-account-lock-UpdateEmail",
                url: "SignInLocked/{action}/{id}",
                defaults: new { controller = "SignIn", action = "UpdateEmail", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-account-lock-PassCodeLock",
                url: "SignInLocked/{action}/{id}",
                defaults: new { controller = "SignIn", action = "PassCodeLock", id = UrlParameter.Optional }
            );

            // Sign in pages
            routes.MapRoute(
                name: "r-sign-in",
                url: "customer-sign-in/{action}/{id}",
                defaults: new { controller = "SignIn", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-sign-general",
                url: "SignIn/{action}/{id}",
                defaults: new { controller = "SignIn", action = "Maintenance", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "r-site-map",
                url: "site-map/{id}",
                defaults: new { controller = "PublicSite", action = "SiteMap", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "r-happy-birthday",
                url: "HappyBirthday/",
                defaults: new { controller = "PublicSite", action = "HappyBirthday", id = UrlParameter.Optional }
            );
            routes.RouteExistingFiles = true;
            routes.MapRoute(
                            name: "r-happy-birthday-default",
                            url: "HappyBirthday/default.aspx",
                            defaults: new { controller = "PublicSite", action = "HappyBirthday", id = UrlParameter.Optional }
                        );
            routes.MapRoute(
                name: "r-plant-a-tree",
                url: "PlantATreeCard/{id}",
                defaults: new { controller = "PublicSite", action = "PlantATreeCard", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-plant-a-tree-default",
                url: "PlantATreeCard/default.aspx",
                defaults: new { controller = "PublicSite", action = "PlantATreeCard", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-holiday-card",
                url: "HolidayeCard/{id}",
                defaults: new { controller = "PublicSite", action = "HolidayeCard", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-holiday-card-default",
                url: "HolidayeCard/default.aspx",
                defaults: new { controller = "PublicSite", action = "HolidayeCard", id = UrlParameter.Optional }
            );

            // for basic static pages (modals, etc....) - a wildcard partial route
            routes.MapRoute(
                name: "static-partials",
                url: "partial/{content}",
                defaults: new { controller = "PublicSite", action = "Partial" }
            );
            routes.MapRoute(
                name: "cms-partials",
                url: "partial/cms/{content}",
                defaults: new { controller = "PublicSite", action = "CMSPartial" }
            );

            routes.MapRoute(
                name: "customer-guarantee",
                url: "customer-guarantee",
                defaults: new { controller = "PublicSite", action = "CMSPartial", content = "customer-guarantee" }
            );
            routes.MapRoute(
                name: "good-credit",
                url: "good-credit",
                defaults: new { controller = "PublicSite", action = "CMSPartial", content = "good-credit" }
            );
            routes.MapRoute(
                name: "r-faq",
                url: "faq/{id}",
                defaults: new { controller = "PublicSite", action = "FAQ", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-rates",
                url: "rates-loan-calculator/{id}",
                defaults: new { controller = "Rates", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "rates-widget",
                url: "rates/Widget/{id}",
                defaults: new { controller = "Rates", action = "Widget", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "r-contact",
                url: "contact-us/{id}",
                defaults: new { controller = "PublicSite", action = "ContactUs", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "accessibility",
                url: "accessibility",
                defaults: new { controller = "PublicSite", action = "Accessibility", id = UrlParameter.Optional }
            );

            MapSuntrustRoutes(routes);

            // most app statuses (Pending, withdraw, etc...)
            routes.MapRoute(
                name: "PreFunding",
                url: "AppStatus/PreFunding/{action}/{id}",
                defaults: new { controller = "PreFunding", action = "Index", id = UrlParameter.Optional }
           );
            routes.MapRoute(
                name: "CancelledThankYou",
                url: "AppStatus/Cancelled/ThankYou",
                defaults: new { controller = "ApplicationStatus", action = "CancelledThankYou", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "appstatus-default",
                url: "AppStatus/{action}/{id}",
                defaults: new { controller = "ApplicationStatus", action = "Index", id = UrlParameter.Optional }
            );

            // apply - incomplete
            routes.MapRoute(
                name: "apply-incomplete",
                url: "apply/incomplete/{action}/{id}",
                defaults: new { controller = "IncompleteApplication", action = "Index", id = UrlParameter.Optional }
            );
            // apply - inquiry
            routes.MapRoute(
                name: "apply-inquiry",
                url: "apply/partner/{action}/{id}",
                defaults: new { controller = "PartnerApplication", action = "Index", id = UrlParameter.Optional }
            );
            // apply - incomplete
            routes.MapRoute(
                name: "apply-lendingtree",
                url: "apply/lendingtree/{action}/{id}",
                defaults: new { controller = "LendingTreeApplication", action = "Index", id = UrlParameter.Optional }
            );
            // apply-add-co-applicant
            routes.MapRoute(
                name: "apply-add-co-applicant",
                url: "apply/Joint/{action}/{id}",
                defaults: new { controller = "AddCoApplicantApplication", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "apply-AccountServices",
                url: "apply/AccountServices/{action}/{id}",
                defaults: new { controller = "ApplyFromAccountServices", action = "Index", id = UrlParameter.Optional }
            );
            // apply 
            routes.MapRoute(
                name: "apply",
                url: "apply/{action}/{id}",
                defaults: new { controller = "Apply", action = "Index", id = UrlParameter.Optional }
            );

            // Errors
            routes.MapRoute(
                name: "error",
                url: "error/{action}/{id}",
                defaults: new { controller = "Error", action = "Index", id = UrlParameter.Optional }
            );

            // AccountServices
            routes.MapRoute(
                name: "Account",
                url: "Account/{action}/{id}",
                defaults: new { controller = "AccountServices", action = "Index", id = UrlParameter.Optional }
            );

            // Custom Landing pages
            routes.MapRoute(
                name: "customLandingPage",
                url: "LandingPage/Custom/{id}",
                defaults: new { controller = "PublicSite", action = "CustomLandingPage", id = UrlParameter.Optional }
            );

            // Landing pages
            routes.MapRoute(
                name: "landingPage",
                url: "LandingPage/{id}",
                defaults: new { controller = "PublicSite", action = "LandingPage", id = UrlParameter.Optional }
            );

            // TODO: e3p0 - remove
            // Catch-all temporary route for hidden responsive web pages
            routes.MapRoute(
                name: "responsiveRedesign",
                url: "r/{action}/{id}",
                defaults: new { controller = "PublicSite", action = "RedesignPage", id = UrlParameter.Optional }
            );

            // shared components on a page (footer, header, nav, etc...)
            routes.MapRoute(
                name: "misc/how-it-works",
                url: "misc/how-it-works",
                defaults: new { controller = "Components", action = "HowItWorks", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "misc/plant-a-tree",
                url: "misc/plant-a-tree",
                defaults: new { controller = "Components", action = "PlantATree", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "components",
                url: "components/{action}/{id}",
                defaults: new { controller = "Components", action = "Index", id = UrlParameter.Optional }
            );

            // JSON services
            routes.MapRoute(
                name: "rateServices",
                url: "services/rates/{action}/{id}",
                defaults: new { controller = "RateCalculator", action = "GetRateTable", id = UrlParameter.Optional },
                namespaces: new[] { "LightStreamWeb.Controllers.Services" }
            );
            
            routes.MapRoute(
                name: "widgetService",
                url: "services/widget/{id}",
                defaults: new { controller = "RateCalculator", action = "Widget", id = UrlParameter.Optional },
                namespaces: new[] { "LightStreamWeb.Controllers.Services" }
            );

            routes.MapRoute(
                name: "loanTermsRequestServices",
                url: "services/GetLatestLoanTermRequest/",
                defaults: new { controller = "LoanTermRequest", action = "GetLatestLoanTermRequest", id = UrlParameter.Optional },
                namespaces: new[] { "LightStreamWeb.Controllers.Services" }
            );

            routes.MapRoute(
                name: "services/lookups",
                url: "services/lookups/{action}/{id}",
                defaults: new { controller = "Lookups", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "LightStreamWeb.Controllers.Services" }
            );
            routes.MapRoute(
                name: "CustomerIdentification",
                url: "CustomerIdentification/{action}/{id}",
                defaults: new { controller = "CustomerIdentification", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "LightStreamWeb.Controllers.Services" }
            );
            routes.MapRoute(
                name: "Profile",
                url: "Profile/{action}/{id}",
                defaults: new { controller = "Profile", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "AccountPreferences",
                url: "AccountPreferences/{action}/{id}",
                defaults: new { controller = "Profile", action = "Index", id = UrlParameter.Optional }
            );

            // cms images
            routes.MapRoute(
                name: "cmsfile",
                url: "content/cmsfile",
                defaults: new { controller = "CMSContent", action = "Index", id = UrlParameter.Optional }
            );

            // home
            routes.MapRoute(
                name: "r-home",
                url: "",
                defaults: new { controller = "PublicSite", action = "RedesignPage", id = UrlParameter.Optional }
            );

            // cms preview
            routes.MapRoute(
                name: "preview",
                url: "Preview/{action}",
                defaults: new { controller = "Preview", action = "Index", id = UrlParameter.Optional }
            );

            GlobalConfiguration.Configure(WebApiConfig.Register);

            routes.MapRoute("Error404", "{*url}", new { controller = "Error", action = "NotFound" });
        }

        private static void MapSuntrustRoutes(RouteCollection routes)
        {
            // Suntrust loan apps
            routes.MapRoute(
                name: "r-suntrust-branch",
                url: "branch/{action}/{id}",
                defaults: new { controller = "SuntrustBranchApplication", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-suntrust-channel-ops",
                url: "ChanOp/{action}/{id}",
                defaults: new { controller = "SuntrustChannelOps", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-suntrust-premier",
                url: "PremierBanking/{action}/{id}",
                defaults: new { controller = "SuntrustPremierApplication", action = "Index", id = UrlParameter.Optional }
            );
            routes.MapRoute(
                name: "r-suntrust-pwn",
                url: "PWM/{action}/{id}",
                defaults: new { controller = "SuntrustPrivateWealthApplication", action = "Index", id = UrlParameter.Optional }
            );


            // and their custom "login" page
            routes.MapRoute(
                name: "SuntrustLogin",
                url: "SuntrustTeammate/{action}",
                defaults: new { controller = "SuntrustSignIn", action = "Index" }
            );

        }

        private static void SetupPublicSitePartialRoutes(RouteCollection routes)
        {
            // customer-testimonials
            routes.MapRoute(
                name: "customer-testimonials",
                url: "customer-testimonials",
                defaults: new { controller = "PublicSite", action = "CustomerTestimonials" }
            );
            routes.MapRoute(
                name: "partial-customer-testimonials",
                url: "partials/customer-testimonials",
                defaults: new { controller = "PublicSite", action = "CMSPartial", content = "customer-testimonials" }
            );

            // pr-media
            foreach (var url in new string[] { "in-the-news", "press-releases", "press-kit" })
            {
                routes.MapRoute(
                    name: "r-" + url,
                    url: url,
                    defaults: new { controller = "PublicSite", action = "PRMedia", tab = url }
                );

                // TEMP for CMS
                routes.MapRoute(
                    name: "r2-" + url,
                    url: "r/" + url,
                    defaults: new { controller = "PublicSite", action = "About", tab = url }
                );

                routes.MapRoute(
                    name: "r-partial-" + url,
                    url: "partials/" + url,
                    defaults: new { controller = "PublicSite", action = "CMSPartial", content = url }
                );
            }
            
            // about us 
            foreach (var url in new string[] { "who-we-are", "about-us", "our-mission", "our-team", "affiliate-program", "pr-media",
                                              "business-partners", "the-anything-loan", "meet-lightstream", "customer-experience-guarantee", "american-forest-event" , "merger-info"} )
            {
                routes.MapRoute(
                    name: "r-" + url,
                    url: url,
                    defaults: new { controller = "PublicSite", action = "About", tab = url }
                );
                // TEMP for CMS
                routes.MapRoute(
                    name: "r2-" + url,
                    url: "r/" + url,
                    defaults: new { controller = "PublicSite", action = "About", tab = url }
                );
                routes.MapRoute(
                    name: "r-partial-" + url,
                    url: "partials/" + url,
                    defaults: new { controller = "PublicSite", action = "CMSPartial", content = url }
                );
            }

            // privacy and security
            foreach (var url in new string[] { "privacy-security", "privacy-policy", "security-policy", "online-privacy", "privacy-overview" })
            {
                routes.MapRoute(
                    name: "r-" + url,
                    url: url,
                    defaults: new { controller = "PublicSite", action = "PrivacySecurity", tab = url }
                );
                // TEMP for CMS
                routes.MapRoute(
                    name: "r2-" + url,
                    url: "r/" + url,
                    defaults: new { controller = "PublicSite", action = "About", tab = url }
                );

                routes.MapRoute(
                    name: "r-partial-" + url,
                    url: "partials/" + url,
                    defaults: new { controller = "PublicSite", action = "CMSPartial", content = url }
                );
            }
            routes.MapRoute(
                         name: "PagerDutyWebhook",
                         url: "PagerDutyWebhook/{action}/{id}",
                         defaults: new { controller = "PagerDutyWebhook", action = "Index", id = UrlParameter.Optional }
                     );

            routes.MapRoute(
                name: "RateBeat",
                url: "rate-beat",
                defaults:
                    AppSettingsHelper.IsRatchMatchOfferEnabled()
                        ? new { controller = "PublicSite", action = "RateBeat" }
                        : new { controller = "PublicSite", action = "Rates" });

            routes.MapRoute(
                name: "partial-RateMatch",
                url: "partials/rate-match",
                defaults: new { controller = "PublicSite", action = "CMSPartial", content = "rate-match" });
        }
    }
}