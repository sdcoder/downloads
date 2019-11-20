using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.App_State;
using QueueApplicationPostResult = LightStreamWeb.Models.Apply.QueueApplicationPostModel.QueueApplicationPostResult;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Common.Xml;
using LightStreamWeb.Helpers;

namespace LightStreamWeb.Models.Apply
{
    [Serializable]
    [XmlRoot("NativeLoanApplication")]
    public class AddCoApplicantModel : NativeLoanApplicationModel
    {
        #region used for JSON serialization, but ignored on app submit to the back end
        [XmlIgnore]
        public bool ApplicantSuppliedOfacQuestions { get; set; }
        [XmlIgnore]
        public string ZipCode { get; set; }
        [XmlIgnore]
        public StateLookup.State State { get; set; }
        [XmlIgnore]
        public bool IsAddCoApplicant
        {
            get
            {
                return true;
            }
        }
        [XmlIgnore]
        public FlagLookup.Flag Discount { get; set; }
        #endregion

        public void Populate(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData, bool isAddCoApplicant)
        {
            this.PopulateApplicationFromLADS(lads);

            // except, this is always a join app
            this.ApplicationType = ApplicationTypeLookup.ApplicationType.Joint;

            this.PopulateApplicantsFromLADS(lads, FeatureSwitch.CitizenshipAndNRAQuestionShouldBeDisplayed, isAddCoApplicant);
            this.PopulateOtherIncomeFromLADS(lads);
            this.PopulateCombinedFinancialsFromLADS(lads);
            this.PopulateSubjectPropertyFromLADS(lads);
            this.PopulateFlagsFromLADS(lads);

            if (this.ApplicationFlags.Any(a => a.FlagType == FlagLookup.Flag.SuntrustPrivateWealth && a.FlagIsOn))
            {
                this.Discount = FlagLookup.Flag.SuntrustPrivateWealth;
            }

            var applicationIfOtherDescriptionRow = lads.ApplicationIfOtherDescription?.SingleOrDefault(
               x => x.IfOtherType == ApplicationIfOtherTypeLookup.ApplicationIfOtherType.ReferredBy);

            // original marketing source
            this.FACTData = new MarketingFACTDataPostData()
            {
                FACT = lads.MarketingSource[0].FirstAgainCodeTrackingId,
                ReferredByDescription = applicationIfOtherDescriptionRow?.IfOtherDescription ?? string.Empty
            };
            
            // carry over marketing info
            if (lads.ApplicationDetail[0].PostingPartner == PostingPartnerLookup.PostingPartner.Generic) // only doing this for generic to address a prod issue and limit potential impact on other apps.
            {
                if (lads.MarketingSupplementalInfo != null && lads.MarketingSupplementalInfo.Count > 0)
                {
                    lads.MarketingSupplementalInfo.ToList().ForEach(msi => MarketingEntityData.Set(msi.MarketingDataEntity, msi.MarketingDataEntityValue));
                }
            }

            // If there's a NLTR, use that
            if (customerData != null)
            {
                CustomerUserIdDataSet.LoanTermsRequestRow ltr = customerData.LoanTermsRequest.GetLatestLoanTermsRequest(lads.ApplicationDetail[0].ApplicationId, LoanTermsRequestTypeLookup.LoanTermsRequestType.NLTR);
                if (ltr != null)
                {
                    this.LoanAmount = ltr.AmountMinusFees;
                    this.LoanTermMonths = ltr.TermMonths;
                    this.PurposeOfLoan.Type = ltr.PurposeOfLoan.IsSecured() ? ltr.PurposeOfLoan.GetUnsecuredPurpose() : ltr.PurposeOfLoan;
                }
                else // secured auto to unsecured is not a NLTR, but does have a loan terms request
                {
                    var approvedTerms = customerData.LoanTermsRequest.GetCurrentApprovedLoanTerms(lads.ApplicationDetail[0].ApplicationId);
                    if (approvedTerms != null)
                    {
                        this.LoanAmount = approvedTerms.AmountMinusFees;
                        this.LoanTermMonths = approvedTerms.TermMonths;
                        this.PurposeOfLoan.Type = approvedTerms.PurposeOfLoan.IsSecured() ? approvedTerms.PurposeOfLoan.GetUnsecuredPurpose() : approvedTerms.PurposeOfLoan;
                    }
                }
            }

            // JSON specific
            var residence = lads.PrimaryApplicant.GetApplicantPrimaryResidence();
            if (residence != null)
            {
                ZipCode = residence.ZipCode;
                State = residence.State;
            }

            // if the OFAC data is missing (pipeline accounts), set a flag so we can supply it 
            var primary = Applicants.FirstOrDefault(a => a.Type == ApplicantTypeLookup.ApplicantType.Primary);
            if (primary != null && primary.Residence != null && primary.Residence.Citizenships != null && primary.Residence.Citizenships.Any())
            {
                ApplicantSuppliedOfacQuestions = true;
            }
        }
    }
}