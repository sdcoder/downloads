using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using LightStreamWeb.Models.Shared;

namespace LightStreamWeb.Models.AccountServices
{
    public class ProfilePageModel : BaseLightstreamPageModel, IPageWithHeading
    {
        protected GetAccountInfoResponse _accountInfo = null;

        public ProfilePageModel(GetAccountInfoResponse accountInfo)
        {
            _accountInfo = accountInfo;
            Heading = "Profile";
        }

        public ProfilePageModel(string bodyClass)
        {
            BodyClass = bodyClass;
        }

        public string Heading { get; set; }

        public bool HasCoApplicant
        {
            get
            {
                var coApplicantInfo = _accountInfo.CustomerUserIdDataSet.ContactInfo.CoApplicantInfo;
                return coApplicantInfo != null;
            }
        }
    }
}