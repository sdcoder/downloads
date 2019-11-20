using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Web.UI;
using FirstAgain.Domain.ServiceModel.Client;

namespace LightStreamWeb.Models.Apply
{
    [Serializable]
    [XmlRoot("NativeLoanApplication")]
    public class AccountServicesAppModel : NativeLoanApplicationModel
    {
        #region used for JSON serialization, but ignored on app submit to the back end
        [XmlIgnore]
        public bool CanUpdateContactInfo { get; set; }
        [XmlIgnore]
        public string ZipCode { get; set; }
        [XmlIgnore]
        public StateLookup.State State { get; set; }
        [XmlIgnore]
        public bool IsAddCoApplicant
        {
            get
            {
                return false;
            }
        }
        [XmlIgnore]
        public bool IsAccountServices
        {
            get
            {
                return true;
            }
        }
        [XmlIgnore]
        public bool MoreThanOneSSNExists { get; set; }
        [XmlIgnore]
        public string CoBorrowerName { get; set; }
        [XmlIgnore]
        public string BorrowerName { get; set; }
        [XmlIgnore]
        public ReApplyApplicationTypeLookup.ReApplyApplicationType ReApplyApplicationType { get; set; }
        [XmlIgnore]
        public bool[] ApplicantIsPrePopulated { get; set; }
        [XmlIgnore]
        public FlagLookup.Flag Discount { get; set; }

        
        public class ReApplyApplicationOption
        {
            public ReApplyApplicationOption() { }

            public ReApplyApplicationOption(string id, string name)
            {
                this.id = id;
                this.name = name;
            }
            public string id { get; set; }
            public string name { get; set; }
        }
        [XmlIgnore]
        public List<ReApplyApplicationOption> ReApplyApplicationTypes { get; set; }
        #endregion


        private void PopulateJointWithSSN1andSSN2(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData)
        {
            this.InitApplicantObjects();
            ApplicationType = ApplicationTypeLookup.ApplicationType.Joint;
            ApplicantIsPrePopulated = new bool[] { true, true };

            PopulateFromApplicantRowOrAccountInfo(this.Applicants.First(), ApplicantTypeLookup.ApplicantType.Primary, customerData.ContactInfo.ApplicantInfo, lads);
            PopulateFromApplicantRowOrAccountInfo(this.Applicants.Last(), ApplicantTypeLookup.ApplicantType.Secondary, customerData.ContactInfo.CoApplicantInfo, lads);

        }

        private void PopulateIndividualWithSSN1(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData)
        {
            this.InitApplicantObjects();
            ApplicationType = ApplicationTypeLookup.ApplicationType.Individual;
            ApplicantIsPrePopulated = new bool[] { true, false };

            PopulateFromApplicantRowOrAccountInfo(this.Applicants.First(), ApplicantTypeLookup.ApplicantType.Primary, customerData.ContactInfo.ApplicantInfo, lads);
        }
        private void PopulateIndividualWithSSN2(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData)
        {
            this.InitApplicantObjects();
            ApplicationType = ApplicationTypeLookup.ApplicationType.Individual;
            ApplicantIsPrePopulated = new bool[] { true, false };

            PopulateFromApplicantRowOrAccountInfo(this.Applicants.First(), ApplicantTypeLookup.ApplicantType.Primary, customerData.ContactInfo.CoApplicantInfo, lads);
        }

        private LoanApplicationDataSet.ApplicantRow FindApplicantRow(LoanApplicationDataSet lads, AccountContactInfo.AccountInfo contactInfo)
        {
            LoanApplicationDataSet correctLads = lads;
            if (lads.Application[0].ApplicationId != contactInfo.ApplicationId)
            {
                correctLads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(contactInfo.ApplicationId);
            }

            return correctLads.Applicant.FirstOrDefault(a => a.ApplicantId == contactInfo.ApplicantId);
        }

        private void PopulateFromApplicantRowOrAccountInfo(ApplicantPostData applicantPostData, ApplicantTypeLookup.ApplicantType applicantType, AccountContactInfo.AccountInfo contactInfo, LoanApplicationDataSet lads)
        {
            var applicantRow = FindApplicantRow(lads, contactInfo);
            if (applicantRow != null)
            {
                applicantPostData.PopulateApplicantFromApplicantRow(applicantType, applicantRow);
            }
            else
            {
                applicantPostData.PopulateApplicantFromAccountInfo(applicantType, contactInfo);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="?"></param>
        /// <param name="lads"></param>
        /// <param name="customerData"></param>
        public void Populate(FirstAgain.Web.UI.ReApplyApplicationTypeLookup.ReApplyApplicationType? appType, LoanApplicationDataSet lads, CustomerUserIdDataSet customerData)
        {
            CanUpdateContactInfo = customerData.ContactInfo.CanUpdate;
            ReApplyApplicationTypes = new List<ReApplyApplicationOption>();
            ReApplyApplicationTypes.Add(new ReApplyApplicationOption(ReApplyApplicationTypeLookup.ReApplyApplicationType.NotSelected.ToString(), ""));
            // Modify Application Type drop down if more than one SSN tied to this user id
            if (!customerData.OneAndOnlyOneSSNExists())
            {
                MoreThanOneSSNExists = true;
            }
            BorrowerName = string.Format("{0} {1}", customerData.ContactInfo.ApplicantInfo.FirstName, customerData.ContactInfo.ApplicantInfo.LastName);

            ReApplyApplicationTypes.Add(new ReApplyApplicationOption(ReApplyApplicationTypeLookup.ReApplyApplicationType.IndividualWithSSN1.ToString(), "Individual Application with " + BorrowerName));

            if (customerData.ContactInfo.CoApplicantInfo != null)
            {
                CoBorrowerName = string.Format("{0} {1}", customerData.ContactInfo.CoApplicantInfo.FirstName, customerData.ContactInfo.CoApplicantInfo.LastName);
                ReApplyApplicationTypes.Add(new ReApplyApplicationOption(ReApplyApplicationTypeLookup.ReApplyApplicationType.IndividualWithSSN2.ToString(), "Individual Application with " + CoBorrowerName));

                ReApplyApplicationTypes.Add(new ReApplyApplicationOption(ReApplyApplicationTypeLookup.ReApplyApplicationType.JointWithSSN1andSSN2.ToString(),  "Joint Application with " + BorrowerName + " and " + CoBorrowerName));
            }

            // loan purpose is not populated from previous apps. Default to NotSelected
            ApplicationType = ApplicationTypeLookup.ApplicationType.NotSelected;
            RateLockDate = DateTime.Now;
            this.PurposeOfLoan = new PurposeOfLoanPostData()
            {
                Type = PurposeOfLoanLookup.PurposeOfLoan.NotSelected
            };

            // populate based on re-application type
            ReApplyApplicationType = appType.GetValueOrDefault(ReApplyApplicationTypeLookup.ReApplyApplicationType.NotSelected);
            switch (ReApplyApplicationType)
            {
                case ReApplyApplicationTypeLookup.ReApplyApplicationType.JointWithSSN1andSSN2:
                    PopulateJointWithSSN1andSSN2(lads, customerData);
                    break;

                case ReApplyApplicationTypeLookup.ReApplyApplicationType.IndividualWithSSN1:
                    PopulateIndividualWithSSN1(lads, customerData);
                    break;

                case ReApplyApplicationTypeLookup.ReApplyApplicationType.IndividualWithSSN2:
                    PopulateIndividualWithSSN2(lads, customerData);
                    break;

                default:
                    this.InitApplicantObjects();
                    ApplicantIsPrePopulated = new bool[] { true, false };
                    this.PopulateApplicantsFromLADS(lads);
                    break;
            }

            // PBI 60320 - convert Homemaker to "Blank"
            foreach (var applicant in Applicants)
            {
                if (applicant.Occupation != null && applicant.Occupation.Type == OccupationTypeLookup.OccupationType.Homemaker)
                {
                    applicant.Occupation.Type = OccupationTypeLookup.OccupationType.NotSelected;
                }
            }

            // marketing source - always "account services"
            this.FACTData = new MarketingFACTDataPostData()
            {
                FACT = LoanApplicationDataSet.KnownFACTs.LIGHTSTREAM_NATIVETRAFFIC_ACCOUNTSERVICES
            };

            if (lads.HasFlag(FlagLookup.Flag.DeclineReferralEligible))
            {
                FACTData.IsEligibleForDeclineReferral = true;
            }

            // pre-populate the user id
            this.UserCredentials = new CustomerUserCredentialsPostData()
            {
                 IsTemporary = false,
                 UserId = customerData.UserId
            };

            // JSON specific model properties
            var primary = Applicants.First();
            if (primary != null && primary.Residence != null && primary.Residence.Address != null)
            {
                ZipCode = primary.Residence.Address.ZipCode;
                State = primary.Residence.Address.State;
            }

            
            // Discount
            if (customerData.ApplicationFlag.Any(a => a.Flag == FlagLookup.Flag.SuntrustPrivateWealth && a.FlagIsOn))
            {
                if (this.ApplicationFlags == null)
                {
                    this.ApplicationFlags = new List<ApplicationFlagPostData>();
                }
                this.ApplicationFlags.Add(new ApplicationFlagPostData()
                    {
                        FlagIsOn = true,
                        FlagType = FlagLookup.Flag.SuntrustPrivateWealth
                    });
                Discount = FlagLookup.Flag.SuntrustPrivateWealth;
            }
        }
    }
}