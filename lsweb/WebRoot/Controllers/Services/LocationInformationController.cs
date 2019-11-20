using LightStreamWeb.Helpers;
using LightStreamWeb.Models.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;

namespace LightStreamWeb.Controllers.Services
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class LocationInformationController : Controller
    {
        protected override JsonResult Json(object data, string contentType, System.Text.Encoding contentEncoding, JsonRequestBehavior behavior)
        {
            return new JsonDotNetResult
            {
                Data = data,
                ContentType = contentType,
                ContentEncoding = contentEncoding,
                JsonRequestBehavior = behavior
            };
        }
        //
        // POST: /Services/StateLookup
        public ActionResult StateLookup(ZipCodeValidationModel model)
        {
            model.Validate();
            return new JsonDotNetResult()
            {
                Data = model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        //
        // POST: /Services/StateTaxLookup
        public ActionResult StateTaxLookup(ZipCodeValidationModel model)
        {
            model.GetStateTax();
            return new JsonDotNetResult()
            {
                Data = model,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }


    }
}
