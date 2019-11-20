using System.Web.Mvc;
using FirstAgain.Common.PagerDuty;
using System.Web.SessionState;

namespace LightStreamWeb.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class PagerDutyWebhookController : Controller
    {
        public ActionResult fcfe9d7c151a4ebe8eb66c83267419a4(PagerDutyWebhookPayload webhookPayload)
        {
            PagerDutyService.HandleWebhookPayload(webhookPayload);

            return new EmptyResult();
        }
    }
}
