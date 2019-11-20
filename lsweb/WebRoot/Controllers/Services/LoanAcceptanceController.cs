using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Common.Logging;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.LoanAcceptance;
using FirstAgain.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    public class LoanAcceptanceController : Controller
    {
        //
        // GET: /Services/LoanAcceptance/NonApplicantSpouseData
        [HttpGet]
        [InjectLoanApplicationDataSet]
        public ActionResult NonApplicantSpouseData(LoanApplicationDataSet lads)
        {
            try
            {
                NonApplicantSpouseModel model = null;

                if (Session["NonApplicantSpouseModel"] != null)
                {
                    model = (NonApplicantSpouseModel)Session["NonApplicantSpouseModel"];
                    if (model == null || model.Primary == null || model.Primary.Name == null || model.ApplicantName.IsNull())
                    {
                        model = new NonApplicantSpouseModel();
                        NonApplicantSpouseModel.Populate(lads, model);
                    }
                }
                else
                {
                    model = new NonApplicantSpouseModel();
                    NonApplicantSpouseModel.Populate(lads, model);
                }
                return new JsonNetResult()
                {
                    Data = model,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new EmptyResult();
            };
        }

        //
        // POST: /Services/LoanAcceptance/NonApplicantSpouseData
        [HttpPost]
        public ActionResult NonApplicantSpouseData(NonApplicantSpouseModel model)
        {
            Session["NonApplicantSpouseModel"] = model;
            return new EmptyResult();
        }

    }
}
