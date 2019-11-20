using System;
using System.Diagnostics.CodeAnalysis;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;

namespace LightStreamWeb.UnitTests.Builders
{
    [ExcludeFromCodeCoverage]
    public class ApplicantRowBuilder
    {
        private int id = 1;
        private string _firstName = "Test";
        private string _lastName = "User";
        private string _socialSecurityNumber = "001122334";
        private short _applicantTypeId = (short)ApplicantTypeLookup.ApplicantType.Primary;
        private DateTime _dateOfBirth = DateTime.Now;

        public ApplicantRowBuilder WithFirstName(string firstName)
        {
            _firstName = firstName;
            return this;
        }

        public ApplicantRowBuilder WithLastName(string lastName)
        {
            _lastName = lastName;
            return this;
        }

        public ApplicantRowBuilder WithSocialSecurityNumber(string socialSecurityNumber)
        {
            _socialSecurityNumber = socialSecurityNumber;
            return this;
        }

        public ApplicantRowBuilder WithApplicantType(ApplicantTypeLookup.ApplicantType applicantType)
        {
            _applicantTypeId = (short) applicantType;
            return this;
        }

        public CustomerUserIdDataSet.ApplicantRow Build(CustomerUserIdDataSet customerUserIdDataSet, CustomerUserIdDataSet.ApplicationRow applicationRow)
        {
            var applicantRow = customerUserIdDataSet.Applicant.NewApplicantRow();
            applicantRow.ApplicantId = id++;
            applicantRow.ApplicantTypeId = _applicantTypeId;
            applicantRow.ApplicationRow = applicationRow;
            applicantRow.FirstName = _firstName;
            applicantRow.LastName = _lastName;
            applicantRow.SocialSecurityNumber = _socialSecurityNumber;
            applicantRow.DateOfBirth = _dateOfBirth;

            return applicantRow;
        }
    }
}