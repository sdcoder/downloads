using FirstAgain.Domain.ServiceModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers.Services
{
    public class AutosuggestController : Controller
    {
        // GET: /Services/Autosuggest/Occupation
        [OutputCache(Duration = 300, VaryByParam = "*")]
        public ActionResult Occupation()
        {
            return new JsonResult()
            {
                Data = DomainServiceUtilityOperations.GetUserEnteredOccupations(true).Where(a => a.MappedTo != "").OrderByDescending(a => a.Count).Select(x => x.Text).Take(5000),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }
    }
}