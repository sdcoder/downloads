using FirstAgain.Domain.Common;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class ExpiredPageModel: ApplicationStatusPageModel
    {
        public ExpiredPageModel(GetAccountInfoResponse accountInfo)
            : base(accountInfo)
        {
            RateModalDisplay = RateModalDisplayType.NotSelected;
        }
        public ExpiredPageModel(ICurrentApplicationData currentAppData)
            : base(currentAppData)
        {
            RateModalDisplay = RateModalDisplayType.NotSelected;
        }

        public string ProductName
        {
            get
            {
                return (ApplicationResultedFromAddCoApplicant()) ? "Individual loan application" : "loan application";
            }
        }

        public string NextStepsMessage
        {
            get
            {
                return (ApplicationResultedFromAddCoApplicant()) ? FAMessages.AddCoApplicantNewUserIdReminder : string.Empty;
            }
        }

        public DateTime WithdrawDate
        {
            get
            {
                if (_currentApplicationData != null && _currentApplicationData.WithdrawDate.HasValue)
                {
                    return _currentApplicationData.WithdrawDate.Value;
                }
                return Application.GetApplicationDetailRows().Single().WithdrawDate;
            }
        }

        public string CustomerServiceEmail
        {
            get
            {
                return BusinessConstants.Instance.CustomerServiceEmail;
            }
        }

    }
}