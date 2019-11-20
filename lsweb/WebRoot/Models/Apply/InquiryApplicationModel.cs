using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.Customer;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Serialization;
using static FirstAgain.Domain.Lookups.FirstLook.ApplicationPostalAddressTypeLookup;

namespace LightStreamWeb.Models.Apply
{
    [Serializable]
    [XmlRoot("NativeLoanApplication")]
    public class InquiryApplicationModel : NativeLoanApplicationModel
    {
        #region used for JSON serialization, but ignored on app submit to the back end
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
        public bool IsGenericPartner
        {
            get
            {
                return true;
            }
        }

        [XmlIgnore]
        public bool[] ApplicantIsPrePopulated { get; set; }
        #endregion

        public void Populate(LoanApplicationDataSet lads, CustomerUserIdDataSet customerData)
        {
            this.PopulateApplicationFromLADS(lads);
            this.PopulateApplicantsFromLADS(lads);
            //this.PopulateOtherIncomeFromLADS(lads);

            if (lads.ApplicationDetail[0].ApplicationType == ApplicationTypeLookup.ApplicationType.Joint)
            {
                ApplicantIsPrePopulated = new bool[] { true, true };
            }
            else
            {
                ApplicantIsPrePopulated = new bool[] { true, false };
            }

            // if purpose of loan is "other" - do not select it
            if (this.PurposeOfLoan.Type == PurposeOfLoanLookup.PurposeOfLoan.Other)
            {
                this.PurposeOfLoan.Type = PurposeOfLoanLookup.PurposeOfLoan.NotSelected;
            }

            // for other income, it is required to show amounts, but not type or description
            if (this.ApplicationOtherIncome == null)
            {
                this.ApplicationOtherIncome = new List<ApplicationOtherIncomePostData>();
            }

            foreach (var otherIncome in lads.ApplicationOtherIncome)
            {
                this.ApplicationOtherIncome.Add(new ApplicationOtherIncomePostData()
                {
                    Amount = otherIncome.OtherIncomeAmount,
                    IncomeType = otherIncome.OtherIncomeType,
                    GrossedUpAmount = otherIncome.GrossedUpOtherIncomeAmount
                });
            };

            this.PopulateCombinedFinancialsFromLADS(lads);
            this.PopulateSubjectPropertyFromLADS(lads);
            this.PopulateFlagsFromLADS(lads);

            // original marketing source
            this.FACTData = new MarketingFACTDataPostData()
            {
                FACT = lads.MarketingSource[0].FirstAgainCodeTrackingId
            };

            // JSON specific
            var residence = lads.PrimaryApplicant.GetApplicantPrimaryResidence();
            if (residence != null)
            {
                ZipCode = residence.ZipCode;
                State = residence.State;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lads"></param>
        /// <param name="currentUser"></param>
        /// <returns></returns>
        public QueueApplicationPostModel.QueueApplicationPostResult SubmitGenericPartnerApp(LoanApplicationDataSet lads, App_State.ICurrentUser currentUser)
        {
            // remove any data left over from the original post, if they removed an applicant
            CheckForJointToIndividualSwitch(lads); 

            // initialize LADS from app post
            CopyLoanInformationToLADS(lads);
            CopyApplicantsToLADS(lads);
            CopyOfacAnswersToLads(lads);
            CopyEmployerToLADS(lads);
            CopyFinancialInformationToLADS(lads);
            CopyHMDAInformationToLADS(lads);
            CopyUserCredentialsToLADS(lads);
            CopyOtherIncomeToLADS(lads);

            // and DL#
            lads.Applicant.FirstOrDefault(a => a.ApplicantType == ApplicantTypeLookup.ApplicantType.Primary).DriversLicenseLastFour = Applicants.FirstOrDefault(a => a.Type == ApplicantTypeLookup.ApplicantType.Primary).DriversLicenseLastFourDigits;
            if (this.ApplicationType == ApplicationTypeLookup.ApplicationType.Joint)
            {
                lads.Applicant.FirstOrDefault(a => a.ApplicantType == ApplicantTypeLookup.ApplicantType.Secondary).DriversLicenseLastFour = Applicants.FirstOrDefault(a => a.Type == ApplicantTypeLookup.ApplicantType.Secondary).DriversLicenseLastFourDigits;
            }

            // add marketing info for generic partners who allow return offers
            var partner = lads.GetGenericPartner();
            if (partner.ReturnOffers)
            {
                if(currentUser.SelectedLoanTerm != null)
                    SetLoanApplicationMarketingSupplementalInfo(lads, MarketingDataEntityLookup.MarketingDataEntity.SelectedLoanTerm, currentUser.SelectedLoanTerm.ToString());
                if (currentUser.AID != null)
                    SetLoanApplicationMarketingSupplementalInfo(lads, MarketingDataEntityLookup.MarketingDataEntity.AID, currentUser.AID.ToString());
            }

            // complete loan app
            var result = DomainServiceLoanApplicationOperations.CompleteLoanApplication(lads, currentUser.UniqueCookie, currentUser.IPAddress);
            if (result == CompleteLoanApplicationResultEnum.CustomerUserIdExists)
            {
                return QueueApplicationPostModel.QueueApplicationPostResult.ReturnError(LoanAppErrorMessages.ErrorCustomerUserIdExists, redirect: QueueApplicationPostModel.QueueApplicationPostResult.RedirectTo.SecurityInfo);
            }
            return new QueueApplicationPostModel.QueueApplicationPostResult()
            {
                Success = true
            };
        }

        public static void SetLoanApplicationMarketingSupplementalInfo(LoanApplicationDataSet loanApplicationDataSet, MarketingDataEntityLookup.MarketingDataEntity entity, string value)
        {
            if (!loanApplicationDataSet.MarketingSupplementalInfo.Any(a => a.MarketingDataEntity == entity))
            {
                var newRow = loanApplicationDataSet.MarketingSupplementalInfo.NewMarketingSupplementalInfoRow();
                newRow.MarketingDataEntity = entity;
                newRow.MarketingDataEntityValue = value;
                newRow.ApplicationId = loanApplicationDataSet.Application[0].ApplicationId;

                loanApplicationDataSet.MarketingSupplementalInfo.AddMarketingSupplementalInfoRow(newRow);
            }
        }

        private void CheckForJointToIndividualSwitch(LoanApplicationDataSet lads)
        {
            if (ApplicationType == ApplicationTypeLookup.ApplicationType.Individual && lads.Applicant.Count > 1)
            {
                var coapplicant = lads.Applicant.FirstOrDefault(a => a.RowState != System.Data.DataRowState.Deleted && a.ApplicantType != ApplicantTypeLookup.ApplicantType.Primary);
                if (coapplicant != null)
                {
                    var applicantRaceRow = lads.ApplicantRace.FirstOrDefault(a => a.ApplicantId == coapplicant.ApplicantId);
                    if (applicantRaceRow != null)
                    {
                        applicantRaceRow.Delete();
                    }

                    LoanApplicationDataSet.ApplicantEmploymentRow employmentRow = lads.ApplicantEmployment.FindByApplicantOccupationId(coapplicant.GetApplicantOccupation().ApplicantOccupationId);
                    LoanApplicationDataSet.ApplicantTelecommRow telecommRow = coapplicant.GetApplicantTelecommByType(TelecommTypeLookup.TelecommType.WorkPhone);
                    LoanApplicationDataSet.ApplicantPostalAddressRow addressRow = lads.ApplicantPostalAddress.FindByApplicantIdPostalAddressTypeId(coapplicant.ApplicantId, (short)PostalAddressTypeLookup.PostalAddressType.PrimaryEmployer);
                    if (employmentRow != null)
                    {
                        employmentRow.Delete();
                    }
                    if (telecommRow != null)
                    {
                        telecommRow.Delete();
                    }
                    if (addressRow != null)
                    {
                        addressRow.Delete();
                    }
                    coapplicant.Delete();
                }
            }
        }

        private void CopyOfacAnswersToLads(LoanApplicationDataSet lads)
        {
            Applicants.ForEach(applicant =>
            {
                var applicantRow = lads.Applicant.FirstOrDefault(a => a.ApplicantType == applicant.Type);

                // Copy ofac Countries
                if (applicant.Residence.Citizenships != null)
                {
                    var ofacCountryRows = applicantRow.GetApplicantOfacCountryRows();

                    if (ofacCountryRows == null || !ofacCountryRows.Any())
                    {
                        applicant.Residence.Citizenships.ForEach(ofacCountry =>
                        {
                            var newOfacCountryRow = applicantRow.NewRelatedApplicantOfacCountryRow();
                            newOfacCountryRow.Country = ofacCountry;

                            lads.ApplicantOfacCountry.AddApplicantOfacCountryRow(newOfacCountryRow);
                        });
                    }
                    else
                    {
                        foreach (LoanApplicationDataSet.ApplicantOfacCountryRow existingRow in ofacCountryRows)
                        {
                            var index = applicant.Residence.Citizenships.IndexOf(existingRow.Country);
                            existingRow.Country = applicant.Residence.Citizenships.ElementAt(index);
                        }
                    }
                }
                

                // Copy non-residency selection
                var residencyFlag = applicantRow.GetApplicantFlagRows().FirstOrDefault(f => f.ApplicantFlag == ApplicantFlagLookup.ApplicantFlag.NonResidentAlien);
                if (residencyFlag.IsNull())
                {
                    if (applicant.Residence.Citizenships != null && applicant.Residence.Citizenships.Any())
                    {
                        residencyFlag = applicantRow.NewRelatedApplicantFlagRow();
                        residencyFlag.ApplicantFlag = ApplicantFlagLookup.ApplicantFlag.NonResidentAlien;
                        residencyFlag.FlagIsOn = applicant.Residence.IsNonResident;
                        lads.ApplicantFlag.AddApplicantFlagRow(residencyFlag);
                    }
                }
                else
                {
                    residencyFlag.FlagIsOn = applicant.Residence.IsNonResident;
                }
            });
        }

        private void CopyHMDAInformationToLADS(LoanApplicationDataSet lads)
        {
            if (HmdaComplianceProperty != null)
            {
                LoanApplicationDataSet.ApplicationPostalAddressRow addressRow = lads.Application[0].GetApplicationPostalAddressByType(ApplicationPostalAddressType.ImprovedProperty);
                if (addressRow == null)
                {
                    addressRow = addressRow = lads.Application[0].NewRelatedApplicationPostalAddressRow();
                    addressRow.ApplicationPostalAddressType = ApplicationPostalAddressType.ImprovedProperty;
                    this.PopulateHmdaComplianceRow(addressRow);
                    lads.ApplicationPostalAddress.AddApplicationPostalAddressRow(addressRow);
                }
                else
                {
                    this.PopulateHmdaComplianceRow(addressRow);
                }

            }

            // check for changes required if changed from home improvement to other
            if (!lads.ApplicationDetail[0].PurposeOfLoan.IsHomeImprovement() 
                && lads.ApplicationPostalAddress.Any(a => a.RowState != DataRowState.Deleted && a.ApplicationPostalAddressType == ApplicationPostalAddressType.ImprovedProperty))
            {
                lads.ApplicationPostalAddress.Single(a =>a.ApplicationPostalAddressType == ApplicationPostalAddressType.ImprovedProperty).Delete();
            }

        }

        private void CopyFinancialInformationToLADS(LoanApplicationDataSet lads)
        {
            if (CombinedFinancials != null)
            {
                if (CombinedFinancials.HomeEquity.HasValue)
                {
                    lads.SetApplicationAsset(ApplicationAssetTypeLookup.ApplicationAssetType.HomeEquity, CombinedFinancials.HomeEquity.Value);
                }
                if (CombinedFinancials.LiquidAssets.HasValue)
                {
                    lads.SetApplicationAsset(ApplicationAssetTypeLookup.ApplicationAssetType.LiquidAssets, CombinedFinancials.LiquidAssets.Value);
                }
                if (CombinedFinancials.RetirementAssets.HasValue)
                {
                    lads.SetApplicationAsset(ApplicationAssetTypeLookup.ApplicationAssetType.RetirementAssets, CombinedFinancials.RetirementAssets.Value);
                }
                if (CombinedFinancials.MonthlyHousingCosts.HasValue)
                {
                    lads.ApplicationDetail[0].MonthlyHousingCost = CombinedFinancials.MonthlyHousingCosts.Value;
                }
            }
        }

        private void CopyOtherIncomeToLADS(LoanApplicationDataSet lads)
        {
            decimal grossUpFactor = BusinessConstants.Instance.TaxExemptGrossUpFactor;
            LoanApplicationDataSet.ApplicationOtherIncomeRow[] otherIncomeRows = lads.Application[0].GetApplicationOtherIncomeRows();

            int rowId = 0;
            if (ApplicationOtherIncome != null)
            {
                foreach (var postedOtherIncome in ApplicationOtherIncome)
                {
                    bool newOtherIncomeRow = false;
                    LoanApplicationDataSet.ApplicationOtherIncomeRow oir = null;
                    // Check is for any existing other income rows. This could be the situation for
                    // partner posted (inquiry) apps
                    if (otherIncomeRows != null && otherIncomeRows.Length > rowId)
                    {
                        oir = otherIncomeRows[rowId];
                    }
                    else
                    {
                        oir = lads.Application[0].NewRelatedApplicationOtherIncomeRow();
                        newOtherIncomeRow = true;
                    }

                    oir.OtherIncomeAmount = oir.GrossedUpOtherIncomeAmount = postedOtherIncome.Amount;
                    oir.OtherIncomeType = postedOtherIncome.IncomeType;
                    if (string.IsNullOrEmpty(postedOtherIncome.OtherIncomeDescription))
                    {
                        oir.SetIncomeSourceDescriptionNull();
                    }
                    else
                    {
                        oir.IncomeSourceDescription = postedOtherIncome.OtherIncomeDescription;
                    }

                    if (oir.IsTaxExempt)
                    {
                        oir.GrossedUpOtherIncomeAmount *= grossUpFactor;
                    }

                    if (newOtherIncomeRow)
                    {
                        lads.ApplicationOtherIncome.AddApplicationOtherIncomeRow(oir);
                    }
                    rowId++;
                }
            }

            for (var excessRowId = rowId; excessRowId < otherIncomeRows.Length; excessRowId++)
            {
                // Account for the use case when a submitted inquiry app has other income specified but the customer
                // decides not to complete this information. Instead of deleting the row, set the income type
                // to not selected and make sure to submit a zero amount. This will preserve the initial values
                // in the history
                otherIncomeRows[excessRowId].GrossedUpOtherIncomeAmount = 0M;
                otherIncomeRows[excessRowId].OtherIncomeAmount = 0M;
                otherIncomeRows[excessRowId].OtherIncomeType = OtherIncomeTypeLookup.OtherIncomeType.NotSelected;
                otherIncomeRows[excessRowId].SetIncomeSourceDescriptionNull();
            }

        }

        /// <summary>
        /// used for generic partner - everything can be updated
        /// </summary>
        /// <param name="lads"></param>
        private void CopyApplicantsToLADS(LoanApplicationDataSet lads)
        {
            Applicants.ForEach(applicant =>
            {
                LoanApplicationDataSet.ApplicantRow applicantRow = lads.Applicant.FirstOrDefault(a => a.ApplicantType == applicant.Type);
                if (applicantRow == null)
                {
                    applicantRow = lads.Application[0].NewRelatedApplicantRow();
                    applicantRow.ApplicantType = applicant.Type;
                }

                applicantRow.FirstName = applicant.FirstName;
                applicantRow.MiddleInitial = applicant.MiddleInitial;
                applicantRow.LastName = applicant.LastName;
                applicantRow.DateOfBirth = applicant.DateOfBirth;
                applicantRow.DriversLicenseLastFour = applicant.DriversLicenseLastFourDigits;
                if (applicant.SocialSecurityNumber.IsNotNullOrEmpty())
                {
                    applicantRow.SocialSecurityNumber = applicant.SocialSecurityNumber;
                }

                // If applicant row is new, add it to the table
                if (applicantRow.RowState == System.Data.DataRowState.Detached)
                    lads.Applicant.AddApplicantRow(applicantRow);

                var homeAddressRow = applicantRow.GetApplicantPostalAddressByType(PostalAddressTypeLookup.PostalAddressType.PrimaryResidence);
                if (homeAddressRow == null)
                {
                    homeAddressRow = applicantRow.NewRelatedApplicantPostalAddressRow();
                    PopulateHomeAddressRow(applicant, homeAddressRow);

                    lads.ApplicantPostalAddress.AddApplicantPostalAddressRow(homeAddressRow);
                }
                else
                {
                    PopulateHomeAddressRow(applicant, homeAddressRow);
                }

                var housingStatusRow = applicantRow.GetApplicantHousingStatus();
                if (housingStatusRow != null)
                {
                    housingStatusRow.ApplicantHousingStatus = applicant.Residence.Ownership;
                    housingStatusRow.MonthsAtResidence = applicant.Residence.TimeAtAddress.Months;
                    housingStatusRow.YearsAtResidence = applicant.Residence.TimeAtAddress.Years;
                }
                else
                {
                    housingStatusRow = applicantRow.NewRelatedApplicantHousingStatusRow();
                    housingStatusRow.ApplicantHousingStatus = applicant.Residence.Ownership;
                    housingStatusRow.MonthsAtResidence = applicant.Residence.TimeAtAddress.Months;
                    housingStatusRow.YearsAtResidence = applicant.Residence.TimeAtAddress.Years;
                    lads.ApplicantHousingStatus.AddApplicantHousingStatusRow(housingStatusRow);
                }

                var homeTelecommRow = applicantRow.GetApplicantHomePhone();
                if (homeTelecommRow == null)
                {
                    homeTelecommRow = applicantRow.NewRelatedApplicantTelecommRow();
                    homeTelecommRow.TelecommType = TelecommTypeLookup.TelecommType.HomePhone;
                }
                homeTelecommRow.AreaCode = applicant.Residence.PhoneNumber.AreaCode;
                homeTelecommRow.PreFix = applicant.Residence.PhoneNumber.CentralOfficeCode;
                homeTelecommRow.LineNumber = applicant.Residence.PhoneNumber.LineNumber;
                if (string.IsNullOrEmpty(applicant.Residence.PhoneNumber.Extension))
                    homeTelecommRow.SetExtensionNull();
                else
                    homeTelecommRow.Extension = applicant.Residence.PhoneNumber.Extension;
                if (homeTelecommRow.RowState == System.Data.DataRowState.Detached)
                    lads.ApplicantTelecomm.AddApplicantTelecommRow(homeTelecommRow);

                var emailAddress = applicantRow.GetApplicantHomeEmail();
                if (emailAddress == null)
                {
                    emailAddress = applicantRow.NewRelatedApplicantEmailRow();
                    emailAddress.EmailType = EmailTypeLookup.EmailType.HomeEmail;
                }
                emailAddress.EmailAddress = applicant.EmailAddress;
                emailAddress.IsConfirmed = true;
                if (emailAddress.RowState == System.Data.DataRowState.Detached)
                    lads.ApplicantEmail.AddApplicantEmailRow(emailAddress);

                if (applicant.Occupation != null
                    && (
                        (applicant.Occupation.Type == OccupationTypeLookup.OccupationType.EmployedByOther || applicant.Occupation.Type == OccupationTypeLookup.OccupationType.EmployedBySelf)
                        || (applicant.Occupation.Employer != null && applicant.Occupation.Employer.Address != null && applicant.Occupation.Employer.Address.AddressLine.IsNotNullOrEmpty())
                        )
                    )
                {
                    var employerAddressRow = applicantRow.GetApplicantPostalAddressByType(PostalAddressTypeLookup.PostalAddressType.PrimaryEmployer);
                    if (employerAddressRow == null)
                    {
                        employerAddressRow = applicantRow.NewRelatedApplicantPostalAddressRow();
                        PopulateEmployerAddressRow(applicant, employerAddressRow);

                        lads.ApplicantPostalAddress.AddApplicantPostalAddressRow(employerAddressRow);
                    }
                    else
                    {
                        PopulateEmployerAddressRow(applicant, employerAddressRow);
                    }
                }

            });
        }

        /// <summary>
        /// used for lending tree, copy only the applicant employer stuff
        /// </summary>
        /// <param name="lads"></param>
        private void CopyEmployerToLADS(LoanApplicationDataSet lads)
        {
            Applicants.ForEach(applicant =>
            {
                var applicantRow = lads.Applicant.First(a => a.ApplicantType == applicant.Type);
                var occupationRow = applicantRow.GetApplicantOccupation();

                if (applicant.Occupation != null)
                {
                    if (occupationRow == null)
                        occupationRow = applicantRow.NewRelatedApplicantOccupationRow();
                    occupationRow.OccupationType = applicant.Occupation.Type;
                    occupationRow.OccupationDescription = string.Empty;

                    // if added applicant occupation row, add it to the table
                    if (occupationRow.RowState == System.Data.DataRowState.Detached)
                        lads.ApplicantOccupation.AddApplicantOccupationRow(occupationRow);
                }

                if (applicant.Occupation.Type == OccupationTypeLookup.OccupationType.EmployedByOther || applicant.Occupation.Type == OccupationTypeLookup.OccupationType.EmployedBySelf ||
                    (applicant.Occupation.Employer != null && applicant.Occupation.Employer.Address != null && applicant.Occupation.Employer.EmployerName.IsNotNull()))
                {
                    occupationRow.OccupationDescription = applicant.Occupation.OccupationDescription ?? string.Empty;

                    var employmentRow = occupationRow.GetApplicantEmploymentRows().FirstOrDefault();
                    if (employmentRow == null)
                    {
                        employmentRow = applicantRow.GetApplicantOccupation().NewRelatedApplicantEmploymentRow();
                        CopyEmployerToEmploymentRow(applicant, employmentRow);
                        lads.ApplicantEmployment.AddApplicantEmploymentRow(employmentRow);
                    }
                    else
                    {
                        CopyEmployerToEmploymentRow(applicant, employmentRow);
                    }

                    var employerAddressRow = applicantRow.GetApplicantPostalAddressByType(PostalAddressTypeLookup.PostalAddressType.PrimaryEmployer);
                    if (employerAddressRow == null)
                    {
                        employerAddressRow = applicantRow.NewRelatedApplicantPostalAddressRow();
                        PopulateEmployerAddressRow(applicant, employerAddressRow);

                        lads.ApplicantPostalAddress.AddApplicantPostalAddressRow(employerAddressRow);
                    }
                    else
                    {
                        PopulateEmployerAddressRow(applicant, employerAddressRow);
                    }

                    var employerPhoneRow = applicantRow.GetApplicantTelecommByType(TelecommTypeLookup.TelecommType.WorkPhone);
                    if (employerPhoneRow == null)
                    {
                        employerPhoneRow = applicantRow.NewRelatedApplicantTelecommRow();

                        PopulateEmployerPhoneRow(applicant, employerPhoneRow);

                        lads.ApplicantTelecomm.AddApplicantTelecommRow(employerPhoneRow);
                    }
                    else
                    {
                        PopulateEmployerPhoneRow(applicant, employerPhoneRow);
                    }

                }
                else  // retired / student / homemaker
                {
                    if (applicant.Occupation.Employer == null || applicant.Occupation.Employer.EmployerName.IsNullOrEmpty())
                    {
                        var employerAddressRow = applicantRow.GetApplicantPostalAddressByType(PostalAddressTypeLookup.PostalAddressType.PrimaryEmployer);
                        if (employerAddressRow != null)
                        {
                            employerAddressRow.Delete();
                        }
                        var employerPhoneRow = applicantRow.GetApplicantTelecommByType(TelecommTypeLookup.TelecommType.WorkPhone);
                        if (employerPhoneRow != null)
                        {
                            employerPhoneRow.Delete();
                        }
                        var employmentRow = occupationRow.GetApplicantEmploymentRows().FirstOrDefault();
                        if (employmentRow != null)
                        {
                            employmentRow.Delete();
                        }
                    }
                }
            });
        }

        private void PopulateEmployerPhoneRow(ApplicantPostData applicant, LoanApplicationDataSet.ApplicantTelecommRow employerPhoneRow)
        {
            if (applicant != null && applicant.Occupation != null && applicant.Occupation.Employer != null && applicant.Occupation.Employer.PhoneNumber != null)
            {
                employerPhoneRow.TelecommType = TelecommTypeLookup.TelecommType.WorkPhone;
                employerPhoneRow.AreaCode = applicant.Occupation.Employer.PhoneNumber.AreaCode;
                employerPhoneRow.PreFix = applicant.Occupation.Employer.PhoneNumber.CentralOfficeCode;
                employerPhoneRow.LineNumber = applicant.Occupation.Employer.PhoneNumber.LineNumber;
                employerPhoneRow.Extension = applicant.Occupation.Employer.PhoneNumber.Extension;
            }
        }

        private static void CopyEmployerToEmploymentRow(ApplicantPostData applicant, LoanApplicationDataSet.ApplicantEmploymentRow employmentRow)
        {
            employmentRow.EmployerName = applicant.Occupation.Employer.EmployerName;
            employmentRow.GrossAnnualIncome = applicant.Occupation.Employer.GrossAnnualIncome;
            employmentRow.Title = "Title";
            employmentRow.YearsWithEmployer = (byte)applicant.Occupation.Employer.TimeWithEmployer.Years;
            employmentRow.MonthsWithEmployer = (byte)applicant.Occupation.Employer.TimeWithEmployer.Months;
        }

        private static void PopulateHomeAddressRow(ApplicantPostData applicant, LoanApplicationDataSet.ApplicantPostalAddressRow addressRow)
        {
            addressRow.PostalAddressType = PostalAddressTypeLookup.PostalAddressType.PrimaryResidence;
            addressRow.AddressLine1 = applicant.Residence.Address.AddressLine;
            addressRow.City = applicant.Residence.Address.City;
            addressRow.State = applicant.Residence.Address.State;
            addressRow.ZipCode = applicant.Residence.Address.ZipCode;

            if (applicant.Residence.Address.SecondaryUnit != null && applicant.Residence.Address.SecondaryUnit.Type != PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected)
            {
                addressRow.SecondaryUnitTypeId = (short)applicant.Residence.Address.SecondaryUnit.Type;
                addressRow.SecondaryUnitValue = applicant.Residence.Address.SecondaryUnit.Value;
            }
            else
            {
                addressRow.SecondaryUnitTypeId = (short)PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected;
                addressRow.SetSecondaryUnitValueNull();
            }
        }

        private static void PopulateEmployerAddressRow(ApplicantPostData applicant, LoanApplicationDataSet.ApplicantPostalAddressRow employerAddressRow)
        {
            employerAddressRow.PostalAddressType = PostalAddressTypeLookup.PostalAddressType.PrimaryEmployer;
            employerAddressRow.AddressLine1 = applicant.Occupation.Employer.Address.AddressLine;
            employerAddressRow.City = applicant.Occupation.Employer.Address.City;
            employerAddressRow.State = applicant.Occupation.Employer.Address.State;
            employerAddressRow.ZipCode = applicant.Occupation.Employer.Address.ZipCode;

            if (applicant.Occupation.Employer.Address.SecondaryUnit != null && applicant.Occupation.Employer.Address.SecondaryUnit.Type != PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected)
            {
                employerAddressRow.SecondaryUnitTypeId = (short)applicant.Occupation.Employer.Address.SecondaryUnit.Type;
                employerAddressRow.SecondaryUnitValue = applicant.Occupation.Employer.Address.SecondaryUnit.Value;
            }
            else
            {
                employerAddressRow.SecondaryUnitTypeId = (short)PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected;
                employerAddressRow.SetSecondaryUnitValueNull();
            }
        }

        private void CopyLoanInformationToLADS(LoanApplicationDataSet lads)
        {
            var applicationDetailRow = lads.ApplicationDetail[0];
            var applicationRow = lads.Application[0];

            if(this.LoanApplicationVersion > 1.0m)
            {
                applicationDetailRow.LoanApplicationVersion = this.LoanApplicationVersion;
            }

            applicationDetailRow.ApplicationType = this.ApplicationType;
            applicationDetailRow.PurposeOfLoan = this.PurposeOfLoan.Type;
            if (!string.IsNullOrEmpty(this.PurposeOfLoan.DescriptionIfOther))
            {
                var appIfOtherRow = applicationRow.GetApplicationIfOtherDescriptionRow(ApplicationIfOtherTypeLookup.ApplicationIfOtherType.PurposeOfLoan);
                if (appIfOtherRow != null)
                {
                    appIfOtherRow.IfOtherDescription = this.PurposeOfLoan.DescriptionIfOther;
                }
                else
                {
                    appIfOtherRow = applicationRow.NewRelatedApplicationIfOtherDescriptionRow();
                    appIfOtherRow.IfOtherDescription = this.PurposeOfLoan.DescriptionIfOther;
                    appIfOtherRow.IfOtherType = ApplicationIfOtherTypeLookup.ApplicationIfOtherType.PurposeOfLoan;
                    lads.ApplicationIfOtherDescription.AddApplicationIfOtherDescriptionRow(appIfOtherRow);
                }
            }
            applicationDetailRow.InterestRate = this.InterestRate;
            applicationDetailRow.TermMonths = this.LoanTermMonths;
            applicationDetailRow.Amount = this.LoanAmount;
            applicationDetailRow.PaymentType = this.LoanPaymentType;
        }

        private void CopyUserCredentialsToLADS(LoanApplicationDataSet lads)
        {
            lads.CustomerUserId[0].PasswordHash = FirstAgain.Domain.SharedTypes.Security.PasswordHash.GetPasswordHash(UserCredentials.Password);
            lads.CustomerUserId[0].IsTemporary = false;
            lads.CustomerUserId[0].CustomerUserId = UserCredentials.UserName;

            if (lads.CustomerUserIdSecurityQuestion.Count == 0)
            {
                LoanApplicationDataSet.CustomerUserIdSecurityQuestionRow customerUserIdSecurityQuestionRow = lads.CustomerUserId[0].NewRelatedCustomerUserIdSecurityQuestionRow(UserCredentials.SecurityQuestionType, UserCredentials.SecurityQuestionAnswer);
                lads.CustomerUserIdSecurityQuestion.AddCustomerUserIdSecurityQuestionRow(customerUserIdSecurityQuestionRow);
            }
            else
            {
                lads.CustomerUserIdSecurityQuestion[0].SecurityQuestion = UserCredentials.SecurityQuestionType;
                lads.CustomerUserIdSecurityQuestion[0].SecurityAnswer = UserCredentials.SecurityQuestionAnswer;
            }
        }

        private bool _isValidEnumValue(short enumValue, short notSeletedValue = 32700)
        {
            return enumValue != 0 && enumValue != notSeletedValue;
        }

    }
}