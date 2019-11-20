using FirstAgain.Common.Logging;
using FirstAgain.Common.Web;
using FirstAgain.Correspondence.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ShawLookups = FirstAgain.Domain.Lookups.ShawData;

namespace LightStreamWeb.Models.AccountServices
{
    public class AccountServicesEDocPageModel
    {
        public string Title { get; set; }
        public string Html { get; set; }

        public void GetExtraPaymentInvoice(string ctx, decimal amount, CustomerUserIdDataSet customerData)
        {
            int? applicationId = WebSecurityUtility.Descramble(ctx);
            if (applicationId.GetValueOrDefault() == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in AccountServicesEDocPageModel.DisplayExtraPaymentInvoice");
                throw new ArgumentException("ApplicationId id is required");
            }

            var applicationRow = customerData.Application.FindByApplicationId(applicationId.Value);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method AccountServicesEDocPageModel.DisplayExtraPaymentInvoice", applicationId.Value));
                throw new HttpException(403, "Access Denied");
            }

            Html = CorrespondenceServiceCorrespondenceOperations.RetrieveInvoiceExtraPayment(applicationId.Value, amount);
            Title = "LightStream - Extra Payment Invoice for Account#" + applicationId.Value;
        }

        internal void DisplayPayOffInvoice(string ctx, DateTime payoffDate, CustomerUserIdDataSet customerData)
        {
            int? applicationId = WebSecurityUtility.Descramble(ctx);
            if (applicationId.GetValueOrDefault() == 0)
            {
                LightStreamLogger.WriteWarning("ApplicationId is zero in AccountServicesEDocPageModel.DisplayExtraPaymentInvoice");
                throw new ArgumentException("ApplicationId id is required");
            }

            var applicationRow = customerData.Application.FindByApplicationId(applicationId.Value);
            if (applicationRow == null)
            {
                LightStreamLogger.WriteWarning(string.Format("Application {0} not found in customer data set, in method AccountServicesEDocPageModel.DisplayExtraPaymentInvoice", applicationId.Value));
                throw new HttpException(403, "Access Denied");
            }

            decimal payoffAmountByInvoice = FirstAgain.LoanServicing.ServiceModel.Client.LoanServicingOperations.GetPayoffQuoteByPaymentMethod(applicationId.Value, payoffDate, ShawLookups.PaymentTypeLookup.PaymentType.Invoice);
            Html = CorrespondenceServiceCorrespondenceOperations.RetrieveInvoiceFinalPayment(applicationId.Value, payoffAmountByInvoice, payoffDate.Date.AddDays(10));
            Title = "LightStream - PayOff Invoice for Account#" + applicationId.Value;
        }
    }
}