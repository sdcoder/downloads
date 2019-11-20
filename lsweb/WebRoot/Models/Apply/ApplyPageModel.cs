using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FirstAgain.Common.Config;
using FirstAgain.Common.Extensions;
using LightStreamWeb.App_State;
using FirstAgain.Domain.ServiceModel.Client;
using CMS = FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using FirstAgain.Domain.Common; 

namespace LightStreamWeb.Models.Apply
{
    public class ApplyPageModel : BaseLightstreamPageModel, IBasicRequirementsModel
    {
        private CoreLoanApplicationPostData _loanAppPostData = null;
        private readonly IThirdPartyRegistrar _thirdPartyRegistrar;
       
        public ApplyPageModel()
            : base()
        {
            _thirdPartyRegistrar = new ThirdPartyRegistrarWrapper();
        }

        public ApplyPageModel(ICurrentUser user) : this(user, new ThirdPartyRegistrarWrapper(), null) { }

        public ApplyPageModel(ICurrentUser user, ContentManager content)
            : this(user, new ThirdPartyRegistrarWrapper(), null)
        {
            base.SetMetadataContent(content.Get<ApplyPage>());
        }
        

        public ApplyPageModel(ICurrentUser user, IThirdPartyRegistrar thirdPartyRegistrar, ICurrentHttpRequest httpRequest)
            : base(user, httpRequest)
        {
            _thirdPartyRegistrar = thirdPartyRegistrar;
            Title = "LightStream Loan Application";
            BodyClass += "apply";
        }

        public ApplyPageModel(ICurrentUser user, IThirdPartyRegistrar thirdPartyRegistrar, ICurrentHttpRequest httpRequest, ContentManager content)
            : base(user, httpRequest)
        {
            _thirdPartyRegistrar = thirdPartyRegistrar;
            Title = "LightStream Loan Application";
            BodyClass += "apply";
        }
        


        public void SetLoanApplicationPostData(CoreLoanApplicationPostData loanAppPostData)
        {
            _loanAppPostData = loanAppPostData;
        }

        public virtual List<SelectListItem> GetMarketingSources()
        {
            return
                (from row in
                     DomainServiceLoanApplicationOperations.GetHowReferredDropDownValues().AsEnumerable()
                 select new SelectListItem()
                 {
                     Text = row.Field<string>("DisplayText"),
                     Value = row.Field<int>("FirstAgainCodeTrackingId").ToString()
                 }).ToList();
        }

        public bool ShouldDisplayReferredBy()
        {
            return (_user.FirstAgainCodeTrackingId == null &&
                    !_user.IsAccountServices &&
                    !_user.AddCoApplicant &&
                    !_user.IsGenericPostingPartner);
        }

        public List<FirstAgain.Domain.SharedTypes.ContentManagement.ApplyPage.BasicDynamicRequirement> LoanPurposeBasedBasicRequirements
        {
            get
            {
                return new ContentManager().Get<CMS.ApplyPage>().DynamicRequirements;
            }
        }

        public string ElectronicDisclosuresCopy
        {
            get
            {
                return new ContentManager().Get<CMS.ApplyPage>().Acknowledgement;
            }
        }

        public string AddCoApplicantIntro
        {
            get
            {
                return new ContentManager().Get<ApplyPage>().AddCoApplicantIntro;
            }
        }

        public bool ShowDeclineReferralOptIn
        {
            get
            {
                return LightStreamWeb.Helpers.MarketingDataSetHelper.IsEligibleForDeclineReferral(_user.FirstAgainCodeTrackingId);
            }
        }

        public string ParentCompanyName
        {
            get
            {
                return BusinessConstants.Instance.MergerNameStatement(); 
            }
        }

        public string GetImpactRadiusJsFilePath()
        {
            MarketingReferrerInfo referrerInfo = WebUser.MarketingReferrerInfo;
            var isImpactRadius = referrerInfo.IsNotNull() && referrerInfo.ReferrerName == "ImpactRadius";
            var jsFilePath = string.Empty;

            if (isImpactRadius)
            {
                var campaignId = string.Empty;
                var actionTrackerId = string.Empty;
                var url = _thirdPartyRegistrar.GetThirdPartySettings("ImpactRadius").GetSetting("url");

                if (!String.IsNullOrEmpty(url))
                {
                    if (referrerInfo.GetDataValue("LinkType") == "IndirectLink")
                    {
                        // Typical URL format: https://d33wwcok8lortz.cloudfront.net/js/1463/3979/irv3.js
                        campaignId = "1463";
                        actionTrackerId = "3979";
                    }
                    else if (referrerInfo.GetDataValue("LinkType") == "DirectLink")
                    {
                        // Typical URL format: https://d33wwcok8lortz.cloudfront.net/js/1695/3980/irv3.js   
                        campaignId = "1695";
                        actionTrackerId = "3980";
                    }

                    jsFilePath = string.Format(@"{0}/js/{1}/{2}/irv3.js", url, campaignId, actionTrackerId);
                }

                WebUser.SetMarketingReferrerInfoCookie(null);
            }

            return jsFilePath;
        }
    }
}