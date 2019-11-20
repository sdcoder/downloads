using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using LightStreamWeb.Models.Components;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.SharedTypes.ContentManagement.SiteContent.PWM;

namespace LightStreamWeb.Models.Apply
{
    public class SunTrustApplyPageModel : ApplyPageModel
    {
        public SunTrustApplyPageModel(ICurrentUser user)
            : base()
        {
            BodyClass = "apply";
        }

        public List<string> SuntrustBasicRequirements
        {
            get
            {
                var applyPage = new ContentManager().Get<FirstAgain.Domain.SharedTypes.ContentManagement.ApplyPage>();
                if (applyPage.SuntrustBasicRequirements != null && applyPage.SuntrustBasicRequirements.Any())
                {
                    return applyPage.SuntrustBasicRequirements;
                }

                return new List<string>(new string[] 
                {
                    "<b>To Apply:</b> You must 1) acknowledge receipt of our <b><a href=\"/electronic-disclosures\" data-popup=\"true\">Statement on the Use of Electronic Records </a></b>(click to review);  2) agree to receive electronic records; 3) agree to use electronic signatures to sign your loan agreement",
                    "<b>If Approved:</b> Prior to receiving loan proceeds, you will be asked to provide a valid Visa or MasterCard credit card (For verification purposes only. No charges will be applied.)"
                });
            }
        }
        public string Tagline { get; set; }

        public IntroPageDisplayMode DisplayMode { get; set; }

        public int TeammateReferralId { get; set; }
        public string TeammateEmailAddress { get; set; }

        public string TeammateReferralToken { get; set; }
        public string EmailAddress { get; set; }
        public bool DisplayPWMCollateralAssetsLink
        {
            get
            {
                if (DisplayMode == IntroPageDisplayMode.PrivateWealth)
                {
                    var content = new ContentManager().Get<PWMCollateralAssets>();
                    return content != null && content.Assets != null && content.Assets.Any();
                }

                return false;
            }
        }
        public bool IsLandingPage
        {
            set
            {
                if (value)
                {
                    BodyClass = "rates";
                }
            }
        }

        // for the landing page
        public RateCalculatorModel Calculator
        {
            get
            {
                var model = new RateCalculatorModel()
                {
                    DisplayCalculator = true,
                    FirstAgainCodeTrackingId = WebUser.FirstAgainCodeTrackingId,
                    Discount = FirstAgain.Domain.Lookups.FirstLook.FlagLookup.Flag.NotSelected,
                    IsSuntrustApplication = true
                };

                if (this.DisplayMode == IntroPageDisplayMode.PremierBanking)
                {
                    model.Discount = FirstAgain.Domain.Lookups.FirstLook.FlagLookup.Flag.SuntrustPremierBanking;
                }
                if (this.DisplayMode == IntroPageDisplayMode.PrivateWealth)
                {
                    model.Discount = FirstAgain.Domain.Lookups.FirstLook.FlagLookup.Flag.SuntrustPrivateWealth;
                }
                return model;
            }
        }


        public string BusinessHours
        {
            get
            {
                var businessHours = BusinessConstants.Instance.BusinessHours.GetFormattedHours(FirstAgain.Common.TimeZoneUS.EasternStandardTime, "-", "to", ":", false);
                return string.Format("{0} & {1}", businessHours[0], businessHours[1]);
            }
        }

        public List<string> AppProcessingHours
        {
            get
            {
                return BusinessConstants.Instance.BusinessHoursAppProcessing.GetFormattedHours(FirstAgain.Common.TimeZoneUS.EasternStandardTime, "-", "to", ":", false);
            }
        }

        public override List<SelectListItem> GetMarketingSources()
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            List<string> stBranchHowYouHeard = new List<string>(new string[] { "", "Branch Signage", "Email Campaign", "Online Banking", "SunTrust Website", "Branch Personnel", "Phone Inquiry" });
            stBranchHowYouHeard.ForEach(item =>
            {
                listItems.Add(new SelectListItem() { Text = item, Value = item.StripWhitespace() });
            });

            MarketingDataSet marketingDataSet = DomainServiceLoanApplicationOperations.GetCachedMarketingData();
            List<MarketingDataSet.FirstAgainCodeTrackDetailRow> factRows = marketingDataSet.GetHowReferredRows().ToList();
            factRows.ForEach(dataRow =>
            {
                if (String.IsNullOrEmpty(dataRow.LkpMarketingPlacementRow.Caption) == false)
                    listItems.Add(new SelectListItem() { Text = dataRow.LkpMarketingPlacementRow.Caption, Value = dataRow.LkpMarketingPlacementRow.Caption.StripWhitespace() });
            });

            return listItems;
        }

        public enum IntroPageDisplayMode
        {
            ReferralForm,
            PremierBanking,
            PrivateWealth,
            ChannelOps
        }
    }
}