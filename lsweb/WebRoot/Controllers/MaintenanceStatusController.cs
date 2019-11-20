using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Models.Marketing;
using LightStreamWeb.Models.Middleware;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    public class MaintenanceStatusController : BaseController
    {

        [Inject]
        public MaintenanceStatusController(ICurrentUser user) : base(user) { }

        [HttpGet]
        [Route("maintenance-status/info")]
        public ActionResult Info()
        {
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    IsInMaintenanceMode = MaintenanceConfiguration.IsInMaintenanceMode
                }
            };
        }
    }
}