using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Models.Marketing;
using Ninject;
using System;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    public class SubscriptionController : BaseController
    {

        [Inject]
        public SubscriptionController(ICurrentUser user) : base(user) { }

        [HttpPost]
        [Route("marketing/subscribe")]
        public ActionResult Subscribe([Bind(Include="FirstName,LastName,Email,ProductInterest")]SubscriptionModel subscribeModel)
        {
            bool isSubscriptionAdded = false;
            bool isInMaintenanceMode = MaintenanceConfiguration.IsInMaintenanceMode;

            try
            {
                if (!isInMaintenanceMode)
                {
                    isSubscriptionAdded = subscribeModel.PostSubscriber();
                }

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Success = true,
                        SubscriptionAdded = isSubscriptionAdded,
                        IsInMaintenanceMode = isInMaintenanceMode
                    }
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        Success = false,
                        SubscriptionAdded = false,
                        IsInMaintenanceMode = isInMaintenanceMode
                    }
                };
            }
        }
    }
}