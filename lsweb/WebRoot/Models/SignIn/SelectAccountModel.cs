using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;

namespace LightStreamWeb.Models.SignIn
{
    public class SelectAccountModel : BaseLightstreamPageModel
    {
        public SelectAccountModel()
        {
            BodyClass = "sign-in";
        }

        public List<UserAccountInfo> UserAccountInfo { get; set; }
        public int SelectedUserId { get; set; }
    }

    public class UserAccountInfo
    {
        public UserAccountInfo(){}
        public int UserId { get; set; }
        // Can be an app id or app nickname (oh boy, gotta love the nickname feature!)
        [MaxLength(100)]
        public string AccountIdentifierLabel { get; set; }
        [MaxLength(100)]
        public string AccountIdentifier { get; set; }
        [MaxLength(100)]
        public string ApplicationType { get; set; }
        [MaxLength(100)]
        public string ApplicationDateLabel { get; set; }
        public DateTime ApplicationDate { get; set; }
        public decimal LoanAmount { get; set; } 
    }

    public static class SelectAccountModelBuilder
    {
        public static SelectAccountModel GetSelectAccountModelFromDataSet(CustomerUserIdDataSet dataset)
        {
            SelectAccountModel model = new SelectAccountModel();
            model.UserAccountInfo = new List<UserAccountInfo>();

            // SSN/UserId search could yield one or multiple apps
            // SSN/UserId search could yield no apps if related to an inquiry or incomplete app
            // App id search should field one customerid
            foreach(CustomerUserIdDataSet.CustomerUserIdXrefApplicationRow row in dataset.CustomerUserIdXrefApplication)
            {
                // Make sure to handle case when user id is not related to any app data- inquiry/incomplete
                CustomerUserIdDataSet.ApplicationRow application = dataset.Application.SingleOrDefault(a => a.ApplicationId == row.ApplicationId);
                CustomerUserIdDataSet.ApplicationDetailRow applicationDetail = dataset.ApplicationDetail.SingleOrDefault(a => a.ApplicationId == row.ApplicationId);

                // Do not return cancelled or terminated apps as an option to the user
                if (application != null && applicationDetail != null
                    && application.ApplicationStatusType.IsNoneOf(ApplicationStatusTypeLookup.ApplicationStatusType.Cancelled))
                {
                    UserAccountInfo uai = new UserAccountInfo() { UserId = row.UserId };

                    if (dataset.ApplicationDetail.SingleOrDefault(ad => ad.ApplicationId == row.ApplicationId).IsApplicationNickNameNull())
                    {
                        uai.AccountIdentifierLabel = "Account Number:";
                        uai.AccountIdentifier = row.ApplicationId.ToString();
                    }
                    else
                    {
                        string accountNickname = dataset.ApplicationDetail.SingleOrDefault(ad => ad.ApplicationId == row.ApplicationId).ApplicationNickName;
                        uai.AccountIdentifierLabel = "Account Nickname:";
                        uai.AccountIdentifier = accountNickname.Trim();
                    }

                    if (dataset.Application.Single(a => a.ApplicationId == row.ApplicationId).ApplicationStatusType == ApplicationStatusTypeLookup.ApplicationStatusType.Funded
                        && dataset.Application.Single(a => a.ApplicationId == row.ApplicationId).GetLoanContractRows().Length > 0)
                    {
                        uai.ApplicationDateLabel = "Funding Date:";
                        uai.ApplicationDate = application.GetLoanContractRows().OrderByDescending(lcr => lcr.LoanOfferId).First().FundingDate;
                    }
                    else
                    {
                        uai.ApplicationDateLabel = "Application Date:";
                        uai.ApplicationDate = application.SubmittedDate;
                    }

                    uai.LoanAmount = application.LoanAmount;
                    uai.ApplicationType = ApplicationTypeLookup.GetCaption(application.ApplicationType);

                    model.UserAccountInfo.Add(uai);
                }                
            }
            
            return model;
        }
    }
}