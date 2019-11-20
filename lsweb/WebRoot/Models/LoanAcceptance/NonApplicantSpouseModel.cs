using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.LoanAcceptance
{
    [Serializable]
    public class NonApplicantSpouseModel
    {
        public NonApplicantSpouseModel()
        {
            StepNumber = 7;
            IsJoint = false;
            Primary = new NonApplicantSpouse();
            Secondary = new NonApplicantSpouse();
            PrimaryApplicantAddresss = new NonApplicantSpouse.NonApplicantSpouseAddress();
            SecondaryApplicantAddresss = new NonApplicantSpouse.NonApplicantSpouseAddress();
        }

        public int StepNumber { get; set; }
        public bool IsJoint { get; set; }
        public string ApplicantName { get; set; }
        public string CoApplicantName { get; set; }
        public bool HasNonApplicantSpouseIncome { get; set; }
        public NonApplicantSpouse Primary { get; set; }
        public NonApplicantSpouse Secondary { get; set; }

        public NonApplicantSpouse.NonApplicantSpouseAddress PrimaryApplicantAddresss { get; set; }
        public NonApplicantSpouse.NonApplicantSpouseAddress SecondaryApplicantAddresss { get; set; }

        internal static void Populate(LoanApplicationDataSet lads, NonApplicantSpouseModel model)
        {
            if (model == null)
            {
                model = new NonApplicantSpouseModel();
            }
            var applicationRow = lads.Application[0];
            model.IsJoint = applicationRow.IsJoint;

            var primaryRow = lads.Applicant.FirstOrDefault(x => x.ApplicantType == FirstAgain.Domain.Lookups.FirstLook.ApplicantTypeLookup.ApplicantType.Primary);
            PopulateApplicant(model.Primary, model.IsJoint, primaryRow);
            model.PrimaryApplicantAddresss = PopulateAddress(primaryRow);
            model.ApplicantName = string.Format("{0} {1}", model.Primary.Name.First, model.Primary.Name.Last);

            var secondaryRow = lads.Applicant.FirstOrDefault(x => x.ApplicantType == FirstAgain.Domain.Lookups.FirstLook.ApplicantTypeLookup.ApplicantType.Secondary);
            PopulateApplicant(model.Secondary, model.IsJoint, secondaryRow);
            model.SecondaryApplicantAddresss = PopulateAddress(secondaryRow);
            model.CoApplicantName = string.Format("{0} {1}", model.Secondary.Name.First, model.Secondary.Name.Last);

            model.HasNonApplicantSpouseIncome = lads.ApplicationOtherIncome.Any(a => a.OtherIncomeType == OtherIncomeTypeLookup.OtherIncomeType.WisconsinSpouseIncome);
        }

        private static void PopulateApplicant(NonApplicantSpouse spouse, bool isJoint, FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.ApplicantRow applicant)
        {
            if (spouse == null)
            {
                spouse = new NonApplicantSpouse();
            }

            if (applicant != null)
            {
                spouse.ApplicantName = string.Format("{0} {1}", applicant.FirstName, applicant.LastName);
                if (applicant.ApplicantType == FirstAgain.Domain.Lookups.FirstLook.ApplicantTypeLookup.ApplicantType.Primary && !isJoint)
                {
                    spouse.Title = "Name of your spouse";
                }
                else
                {
                    spouse.Title = string.Format("Name of {0}'s spouse", spouse.ApplicantName);
                }
            }
        }

        private static NonApplicantSpouse.NonApplicantSpouseAddress PopulateAddress(FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationDataSet.ApplicantRow applicant)
        {
            NonApplicantSpouse.NonApplicantSpouseAddress address = new NonApplicantSpouse.NonApplicantSpouseAddress();

            if (applicant != null)
            {
                var addressRow = applicant.GetApplicantPostalAddressByType(FirstAgain.Domain.Lookups.FirstLook.PostalAddressTypeLookup.PostalAddressType.PrimaryResidence);
                if (addressRow != null)
                {
                    address.AddressLine = addressRow.AddressLine1;
                    address.City = addressRow.City;
                    address.SecondaryUnitType = (PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType)addressRow.SecondaryUnitTypeId;
                    address.SecondaryUnitValue = addressRow.IsSecondaryUnitValueNull() ? string.Empty : addressRow.SecondaryUnitValue;
                    address.State = addressRow.State;
                    address.ZipCode = addressRow.ZipCode;
                }
            }

            return address;
        }
    }
}