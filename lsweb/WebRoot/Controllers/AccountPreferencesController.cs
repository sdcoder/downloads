using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    public class AccountPreferencesController : Controller
    {
        // GET: AccountPreferences
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Profile");
        }
    }
}