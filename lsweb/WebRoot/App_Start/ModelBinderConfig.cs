using FirstAgain.Common.PagerDuty;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.LoanServicing.SharedTypes;
using LightStreamWeb.App_State;
using LightStreamWeb.ModelBinders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb
{
    public class ModelBinderConfig
    {
        public static void RegisterModelBinders()
        {
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(ICurrentApplicationData), new DoNothingModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(CurrentApplicationData), new DoNothingModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(LoanApplicationDataSet), new DoNothingModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(CustomerUserIdDataSet), new DoNothingModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(CustomerApplicationsDates), new DoNothingModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(LoanOfferDataSet), new DoNothingModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(PagerDutyWebhookPayload), new NewtonsoftJsonBinder<PagerDutyWebhookPayload>());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(AccountServicesDataSet), new DoNothingModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(FirstAgain.LoanServicing.SharedTypes.BusinessCalendarDataSet), new DoNothingModelBinder());
            System.Web.Mvc.ModelBinders.Binders.Add(typeof(FirstAgain.Domain.SharedTypes.LoanApplication.BusinessCalendarDataSet), new DoNothingModelBinder());
            
        }
    }
}