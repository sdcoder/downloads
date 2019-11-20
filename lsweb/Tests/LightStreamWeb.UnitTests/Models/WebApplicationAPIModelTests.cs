using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.UnitTests.Models
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class WebApplicationAPIModelTests
    {
        [Test]
        public void TestSanitizeRejectsSecuredAuto()
        {
            var subject = new WebApplicationAPIModel();
            var securedPurpose = PurposeOfLoanLookup.PurposeOfLoan.AutoRefinancingSecured;
            var unsecuredPurpose = securedPurpose.GetUnsecuredPurpose();


            subject.PurposeOfLoan = securedPurpose;
            subject.Sanitize();

            Assert.AreNotEqual(subject.PurposeOfLoan, securedPurpose);
            Assert.AreEqual(subject.PurposeOfLoan, unsecuredPurpose);
        }

        [Test]
        public void TestSanitizeRejectsJavascript()
        {
            var subject = new WebApplicationAPIModel();
            string htmlString = "<script src='https://attacker.com/bad.js'></script>";

            subject.ApplicantFirstName = htmlString;
            subject.ApplicantLastName = htmlString;
            subject.Sanitize();

            Assert.AreNotEqual(htmlString, subject.ApplicantFirstName);
            Assert.AreNotEqual(htmlString, subject.ApplicantLastName);
        }

        [Test]
        public void TestSanitizeRejectsHtml()
        {
            var subject = new WebApplicationAPIModel();
            string htmlString = "<img src='http://attacker.com/bad.png'>";

            subject.ApplicantFirstName = htmlString;
            subject.ApplicantLastName = htmlString;
            subject.Sanitize();

            Assert.AreNotEqual(htmlString, subject.ApplicantFirstName);
            Assert.AreNotEqual(htmlString, subject.ApplicantLastName);
        }

        [Test]
        public void TestSanitizeCorrectsZipCodes()
        {
            var subject = new WebApplicationAPIModel();

            subject.ZipCode = "WORD!";
            subject.Sanitize();
            Assert.AreEqual("", subject.ZipCode);

            subject.ZipCode = "1234567890";
            subject.Sanitize();
            Assert.AreEqual("12345", subject.ZipCode);
        }

        [Test]
        [TestCase("CA", StateLookup.State.California)]
        [TestCase("North Dakota", StateLookup.State.NorthDakota)]
        [TestCase("NorthDakota", StateLookup.State.NorthDakota)]
        public void TestSanitizeCorrectsStates(string sState, StateLookup.State eState)
        {
            var subject = new WebApplicationAPIModel();

            subject.ApplicantResidenceAddressState = sState;
            subject.Sanitize();
            Assert.AreEqual(eState.ToString(), subject.ApplicantResidenceAddressState);
        }

        [Test]
        [TestCase("CA", StateLookup.State.California)]
        [TestCase("North Dakota", StateLookup.State.NorthDakota)]
        [TestCase("NorthDakota", StateLookup.State.NorthDakota)]
        public void TestSanitizeCorrectsCoAppStates(string sState, StateLookup.State eState)
        {
            var subject = new WebApplicationAPIModel();

            subject.CoApplicantResidenceAddressState = sState;
            subject.Sanitize();
            Assert.AreEqual(eState.ToString(), subject.CoApplicantResidenceAddressState);
        }
    }
}
