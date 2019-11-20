using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace LightStreamWeb.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class CMSContentController : Controller
    {
        // GET: CMSContent
        [OutputCache(Duration = 31536000, VaryByParam = "*")] // One year
        public ActionResult Index()
        {
            return new CMSContentActionResult();
        }
    }
}