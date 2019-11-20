using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.LoanAcceptance
{
    [Serializable]
    public class NonApplicantSpouse
    {
        public NonApplicantSpouse()
        {
            Address = new NonApplicantSpouseAddress();
            ApplicantAddress = new NonApplicantSpouseAddress();
            Name = new NonApplicantSpouseName();
        }

        public bool? HasNonApplicantSpouse { get; set; }
        public bool? SpouseNotInState { get; set; }
        public bool SameAddress { get; set; }
        public string ApplicantName { get; set; }
        public string Title { get; set; }

        public NonApplicantSpouseName Name { get; set; }
        public NonApplicantSpouseAddress Address { get; set; }
        public NonApplicantSpouseAddress ApplicantAddress { get; set; }

        [Serializable]
        public class NonApplicantSpouseName
        {
            public string First { get; set; }
            public string MI { get; set; }
            public string Last { get; set; }
        }

        [Serializable]
        public class NonApplicantSpouseAddress
        {
            public NonApplicantSpouseAddress()
            {
                SecondaryUnitType = PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType.NotSelected;
                State = StateLookup.State.NotSelected;
            }

            public string AddressLine { get; set; }
            public string SecondaryUnitValue { get; set; }
            public PostalAddressSecondaryUnitTypeLookup.PostalAddressSecondaryUnitType SecondaryUnitType { get; set; }

            public string City { get; set; }
            public StateLookup.State State { get; set; }
            public string ZipCode { get; set; }
        }
    }
}