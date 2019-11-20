using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.Models.ApplicationStatus;
using LightStreamWeb.Models.SignIn;
using System;
using System.Linq;
using LightStreamWeb.Tests.Mocks;
using LightStreamWeb.App_Start;
using LightStreamWeb.App_State;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class ApplicationStatusTests
    {
        [OneTimeSetUp]
        public static void NinjectSetup()
        {
            NinjectWebCommon.Start();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentHttpRequest>().To<MockHttpRequest>();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentUser>().To<MockUser>();
        }

        [Test]
        [Ignore("not a unit test")]
        public void StatusCheck()
        {
            new StatusCheckModel().GetNextCheck(10921169, FirstAgain.Domain.Lookups.FirstLook.ApplicationStatusTypeLookup.ApplicationStatusType.Terminated);
        }
        [Test]
        [Ignore("not a unit test")]
        public void PendingV()
        {
            var vr_qatemplate = GetVerificationReqestsForAccount("qatemplate");
            Assert.AreEqual(new DateTime(year:2014,month:8, day:2) .Date, vr_qatemplate.DueDate.Date);
            Assert.AreEqual("document", vr_qatemplate.Description);
            Assert.IsFalse(vr_qatemplate.CollateralInformation.IsRequired());
            Assert.IsFalse(vr_qatemplate.CustomerIdentification.IsRequired());
            Assert.IsTrue(vr_qatemplate.Documents.IsRequired());
            Assert.AreEqual(1, vr_qatemplate.Documents.Items().Count());

            var vr_testprod66 = GetVerificationReqestsForAccount("testprod66");
            Assert.AreEqual(new DateTime(year: 2014, month: 7, day: 28).Date, vr_testprod66.DueDate.Date);
            Assert.AreEqual("identity verification", vr_testprod66.Description);
            Assert.IsFalse(vr_testprod66.CollateralInformation.IsRequired());
            Assert.IsTrue(vr_testprod66.CustomerIdentification.IsRequired());
            Assert.AreEqual("Cindy Lugan", vr_testprod66.CustomerIdentification.Items().First().Caption);
            Assert.AreEqual("Test Tests", vr_testprod66.CustomerIdentification.Items().Skip(1).Single().Caption);
            Assert.IsFalse(vr_testprod66.Documents.IsRequired());
            Assert.AreEqual(1, vr_testprod66.Documents.Items().Count());

            var vr_collateraltest13 = GetVerificationReqestsForAccount("collateraltest13");
            Assert.AreEqual(new DateTime(year: 2014, month: 8, day: 7).Date, vr_collateraltest13.DueDate.Date);
            Assert.AreEqual("document", vr_collateraltest13.Description);
            Assert.IsFalse(vr_collateraltest13.CollateralInformation.IsRequired());
            Assert.IsFalse(vr_collateraltest13.CustomerIdentification.IsRequired());
            Assert.IsTrue(vr_collateraltest13.Documents.IsRequired());
            Assert.AreEqual(1, vr_collateraltest13.Documents.Items().Count());

            var vr_Auto07092014053210 = GetVerificationReqestsForAccount("Auto07092014053210");
            Assert.IsFalse(vr_Auto07092014053210.Any());
            Assert.IsFalse(vr_Auto07092014053210.CollateralInformation.IsRequired());
            Assert.IsFalse(vr_Auto07092014053210.CustomerIdentification.IsRequired());
            Assert.IsFalse(vr_Auto07092014053210.Documents.IsRequired());

            var vr_jun19x1710 = GetVerificationReqestsForAccount("jun19x1710");
            Assert.AreEqual(new DateTime(year: 2014, month: 6, day: 30).Date, vr_jun19x1710.DueDate.Date);
            Assert.AreEqual("document", vr_jun19x1710.Description);
            Assert.IsFalse(vr_jun19x1710.CollateralInformation.IsRequired());
            Assert.IsFalse(vr_jun19x1710.CustomerIdentification.IsRequired());
            Assert.IsTrue(vr_jun19x1710.Documents.IsRequired());
            Assert.AreEqual(1, vr_jun19x1710.Documents.Items().Count());
        }

        [Test]
        [Ignore("not a unit test")]
        public void CounterV()
        {
            var vr_collateraltest12 = GetVerificationReqestsForAccount("collateraltest12");
            Assert.AreEqual(new DateTime(year: 2014, month: 8, day: 7).Date, vr_collateraltest12.DueDate.Date);
            Assert.AreEqual("vehicle verification", vr_collateraltest12.Description);
            Assert.IsTrue(vr_collateraltest12.CollateralInformation.IsRequired());
            Assert.IsFalse(vr_collateraltest12.CustomerIdentification.IsRequired());
            Assert.IsFalse(vr_collateraltest12.Documents.IsRequired());
            Assert.AreEqual(0, vr_collateraltest12.Documents.Items().Count());

            var vr_collateraltest3 = GetVerificationReqestsForAccount("collateraltest3");
            Assert.AreEqual(new DateTime(year: 2014, month: 8, day: 3).Date, vr_collateraltest3.DueDate.Date);
            Assert.AreEqual("vehicle verification and document", vr_collateraltest3.Description);
            Assert.IsTrue(vr_collateraltest3.CollateralInformation.IsRequired());
            Assert.IsFalse(vr_collateraltest3.CustomerIdentification.IsRequired());
            Assert.IsTrue(vr_collateraltest3.Documents.IsRequired());
            Assert.AreEqual(1, vr_collateraltest3.Documents.Items().Count());

            var vr_alugo55 = GetVerificationReqestsForAccount("alugo55");
            Assert.AreEqual(new DateTime(year: 2014, month: 5, day: 17).Date, vr_alugo55.DueDate.Date);
            Assert.AreEqual("documents", vr_alugo55.Description);
            Assert.IsFalse(vr_alugo55.CollateralInformation.IsRequired());
            Assert.IsFalse(vr_alugo55.CustomerIdentification.IsRequired());
            Assert.IsTrue(vr_alugo55.Documents.IsRequired());
            Assert.AreEqual(3, vr_alugo55.Documents.Items().Count());
        }

        private VerificationRequestsModel GetVerificationReqestsForAccount(string userName)
        {
            var accountData = new SignInModel().GetAccountStatus(userName);
            var model = new VerificationRequestsModel();
            Assert.IsNotNull(model);

            var lads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(accountData.ApplicationId);

            model.Populate(accountData.CustomerData, accountData.ApplicationId, FirstAgain.Domain.Lookups.FirstLook.PurposeOfLoanLookup.PurposeOfLoan.NotSelected);
            return model;
        }
    }
}
