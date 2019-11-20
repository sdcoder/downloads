using FirstAgain.Web.UI;
using LightStreamWeb.App_State;
using LightStreamWeb.Filters;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers.Shared
{
    public abstract class BaseApplicationStatusController : BaseController
    {
        [Inject]
        public BaseApplicationStatusController(ICurrentUser user)
            : base(user)
        {
        }

        //
        // GET: /AppStatus/Refresh
        [HttpGet]
        [RequireApplicationId]
        public ActionResult Refresh()
        {
            var accountInfo = WebUser.Refresh();

            return new RedirectResult(StatusRedirect.GetRedirectBasedOnStatus(accountInfo.CustomerUserIdDataSet, WebUser.ApplicationId.Value));
        }

        //
        // POST: /AppStatus/Refresh
        [HttpPost]
        [RequireApplicationId]
        public ActionResult Refresh(string postData)
        {
            var accountInfo = WebUser.Refresh();

            return new EmptyResult();
        }
    }
}