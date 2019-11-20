using FirstAgain.Domain.Common;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using LightStreamWeb.App_State;
using Resources;
using System;
using System.Linq;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class WithdrawnPageModel : ApplicationStatusPageModel
    {
        public WithdrawnPageModel(GetAccountInfoResponse accountInfo)
            : base(accountInfo)
        {
            RateModalDisplay = RateModalDisplayType.NotSelected;
        }
        public WithdrawnPageModel(ICurrentApplicationData applicationData)
            : base(applicationData)
        {
            RateModalDisplay = RateModalDisplayType.NotSelected;
        }


        public string ProductName
        {
            get
            {
                return (ApplicationResultedFromAddCoApplicant()) ? "individual loan application" : "loan application";
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
                return _currentApplicationData?.WithdrawDate ?? Application.GetApplicationDetailRows().Single().WithdrawDate;
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