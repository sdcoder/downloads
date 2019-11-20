using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.Models.AccountServices;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class CancelledPageModel : ApplicationStatusPageModel
    {
        #region constructors
        public CancelledPageModel(CustomerUserIdDataSet cuidds)
            : base(cuidds)
        {
            Populate();
        }

        public CancelledPageModel(GetAccountInfoResponse accountInfo)
            : base(accountInfo)
        {
            Populate();
        }
        #endregion

        public List<string> Statements { get; private set; }
        public bool EnableContactUs { get; private set; }
        public bool IsDisplucateAppCancelled { get; private set; }

        private void Populate()
        {
            RateModalDisplay = ApplicationStatusPageModel.RateModalDisplayType.NotSelected;

            Statements = new List<string>();
            var applicationRow = _customerUserIdDataSet.Application.FindByApplicationId(WebUser.ApplicationId.Value);
            CustomerUserIdDataSet.ApplicationDetailRow[] appdetail = applicationRow.GetApplicationDetailRows();
            var cancelDate = appdetail[0].CancelDate;

            CustomerUserIdDataSet.ApplicationCancelReasonRow[] cancelReasons = applicationRow.GetApplicationCancelReasonRows();
            switch (cancelReasons[0].CancelReasonType)
            {
                // Cancelled due to adding a co-applicant
                case (CancelReasonTypeLookup.CancelReasonType.AddedCoApplicant):
                    Statements.Add(FAMessages.AddCoApplicantNewUserIdReminder);
                    //Response.Redirect(string.Format("AppCancelled.aspx?Auto=true&AddedCoApplicant=true&ctx={0}", WebSecurityUtility.Ctx));
                    break;
                // Duplicate app cancel
                case (CancelReasonTypeLookup.CancelReasonType.DuplicateApp):
                    Statements.Add("We received your application, however, we also have a prior loan request from you which leads us to believe this recent application is a duplicate and, as a result, it has been cancelled.");
                    Statements.Add("If this is not a duplicate application, or if you have any comments/questions, please complete the eform below. We will respond promptly during our normal business hours.");
                    EnableContactUs = true;
                    IsDisplucateAppCancelled = true;
                    //Response.Redirect(string.Format("DuplicateAppCancelled.aspx?ctx={0}", WebSecurityUtility.Ctx));
                    break;
                // Auto cancel
                case (CancelReasonTypeLookup.CancelReasonType.IncompleteApplication):
                case (CancelReasonTypeLookup.CancelReasonType.CounterOfferRefused):
                    Statements.Add(string.Format("Your loan application was cancelled on {0}. If you have any questions, please contact us at <a href=\"tel:{1}\">{1}</a>.",
                        cancelDate.ToString("MMMM d, yyyy"),
                        BusinessConstants.Instance.PhoneNumberMain
                        ));
                    //Response.Redirect(String.Format(redirectAppCancelledPage, "true"));
                    break;
                default:
                    Statements.Add(string.Format("As requested, your loan application was cancelled on {0}. If you have any questions, please contact us at <a href=\"tel:{1}\">{1}</a>.",
                        cancelDate.ToString("MMMM d, yyyy"),
                        BusinessConstants.Instance.PhoneNumberMain
                        ));
                    //Response.Redirect(String.Format(redirectAppCancelledPage, "false"));
                    break;
            }
            Statements.Add("Thank you.");

        }

        public AccountServicesContactUsModel GetContactUsModel()
        {
            var model = new AccountServicesContactUsModel(_customerUserIdDataSet, _accountInfo.ApplicationsDates, WebUser);
            model.ApplicationId = WebUser.ApplicationId;
            return model;
        }

    }
}