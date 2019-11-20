using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Common.Logging;
using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.ENotices;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    public class ENoticesController : BaseController
    {
        [Inject]
        public ENoticesController(ICurrentUser user)
            : base(user)
        {
        }

        //
        // GET: /enotices/modal
        [HttpGet]
        public ActionResult Modal()
        {
            return HttpNotFound();
        }

        //
        // POST: /enotices/validate
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult Validate(CustomerUserIdDataSet customerData, ENoticeSecurityCheckModel model)
        {
            try
            {
                model.Validate(customerData);
                return new JsonNetResult()
                {
                    Data = model
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new EmptyResult();
            }
        }

        //
        // POST: /enotices/load
        [HttpPost]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult Load(CustomerUserIdDataSet customerData, CorrespondenceCategoryLookup.CorrespondenceCategory? SuppressType)
        {
            try
            {
                var model = new ENoticesModalModel(WebUser.ApplicationId.GetValueOrDefault(), customerData, SuppressType);
                if (!model.Docs.Any() && SuppressType.HasValue)
                {
                    // document may not have generated yet. Refresh
                    var accountInfo = WebUser.Refresh();
                    model = new ENoticesModalModel(WebUser.ApplicationId.GetValueOrDefault(), accountInfo.CustomerUserIdDataSet, SuppressType);
                }
                return new JsonNetResult()
                {
                    Data = model
                };
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                return new EmptyResult();
            }
        }

        //
        // GET: /enotices/view
        // displays notice as a partial
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult View(CustomerUserIdDataSet customerData, long id)
        {
            var model = new ENoticeDisplayModel();
            model.LoadEnoticeContent(customerData, id);
            return View(model);
        }

        //
        // GET: /enotices/html
        // displays as a full page
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult Html(CustomerUserIdDataSet customerData, long id)
        {
            var model = new ENoticeDisplayModel();
            model.LoadEnoticeContent(customerData, id);
            return View(model);
        }

        //
        // GET: /ENotices/ViewLoanAgreement
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult ViewLoanAgreement(CustomerUserIdDataSet customerData, int? id, int appId=  0)
        {
            var model = new ENoticeDisplayModel();
            model.LoadLoanAgreement(appId == 0 ? WebUser.ApplicationId.Value : appId, customerData, id);
            return View(model);
        }

        //
        // GET: /enotices/download
        [HttpGet]
        [RequireApplicationId]
        [InjectCustomerUserIdDataSet]
        public ActionResult Download(CustomerUserIdDataSet customerData, long id)
        {
            string fileName;
            byte[] doc = new ENoticeDisplayModel().GetEnoticPdf(customerData, id, out fileName);

            Response.AddHeader("Content-Disposition", string.Format("attachment; filename=\"{0}.pdf\"", fileName));
            return new FileContentResult(doc, "application/pdf");
        }

    }
}
