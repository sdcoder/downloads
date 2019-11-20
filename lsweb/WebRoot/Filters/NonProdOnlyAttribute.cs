using FirstAgain.Domain.Common;
using System.Web.Mvc;


namespace LightStreamWeb.Filters
{
    public class NonProdOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!BusinessConstants.Instance.EnvironmentIsDevOrQA())
            {
                filterContext.Result = new HttpNotFoundResult();
                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}