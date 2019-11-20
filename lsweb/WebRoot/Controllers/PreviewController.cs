using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.ContentManagement.SiteContent.Sitewide;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Models.Apply;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    public class PreviewController : BaseController
    {
        [Inject]
        public PreviewController(ICurrentUser user)
            : base(user)
        {
        }

        //
        // GET: /Apply/PWM/
        // just for CMS preview without having to log in
        public ActionResult PWM()
        {
            return View(viewName: "~/Views/Apply/Suntrust/BasicInfo.cshtml", model: new SunTrustApplyPageModel(WebUser)
            {
                Tagline = "Private Wealth Management Preview",
                DisplayMode = SunTrustApplyPageModel.IntroPageDisplayMode.PrivateWealth
            });
        }

        [HttpGet]
        [Route("Preview/SiteAsset")]
        public ActionResult SiteAsset( )
        {
            var cm = new ContentManager();
            var asset = cm.Get<SiteAsset>();
            return View(asset);
        }
    }
}