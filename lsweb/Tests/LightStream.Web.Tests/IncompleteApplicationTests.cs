using System;
using LightStreamWeb.Models.SignIn;
using LightStreamWeb.Models.Apply;
using FirstAgain.Domain.ServiceModel.Client;
using LightStreamWeb.Tests.Mocks;
using System.Transactions;
using LightStreamWeb.App_Start;
using LightStreamWeb.App_State;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class IncompleteApplicationTests
    {
        [OneTimeSetUp]
        public static void NinjectSetup(TestContext testContext)
        {
            NinjectWebCommon.Start();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentHttpRequest>().To<MockHttpRequest>();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentUser>().To<MockUser>();
        }

        [Test]
        [Ignore("not a unit test")]
        public void TeammateReferral()
        {
            var lads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(88327159);
            Assert.IsFalse(lads.IsReferredViaTeammateConciergeMode());
        }

        [Test]
        [Ignore("not a unit test")]
        public void JointToIndividual()
        {
            var accountData = new SignInModel().GetAccountStatus("jt24zxc6");
            Assert.IsNotNull(accountData);

            var lads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(accountData.ApplicationId);

            var model = new InquiryApplicationModel();
            model.Populate(lads, accountData.CustomerData);

            // switch to individual
            model.ApplicationType = FirstAgain.Domain.Lookups.FirstLook.ApplicationTypeLookup.ApplicationType.Individual;

            // make some credentials
            model.UserCredentials = new FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData.CustomerUserCredentialsPostData()
            {
                 Password = "password1",
                 UserName = "Q" + new Random().Next(100000, 999999).ToString(),
                 SecurityQuestionType = FirstAgain.Domain.Lookups.FirstLook.SecurityQuestionLookup.SecurityQuestion.ChildhoodFriend,
                 SecurityQuestionAnswer  = "password1",
                 IsTemporary = true
            };
            LightStreamWeb.Models.Apply.QueueApplicationPostModel.QueueApplicationPostResult result = null;

            using (var ts = new TransactionScope())
            {
                result = QueueApplicationPostModel.SetDefaultsAndValidate(model);
                if (result.Success)
                {
                    result = QueueApplicationPostModel.ValidateSecurityInfo(model);
                    if (result.Success)
                    {
                        // Generic Partner specific
                        result = model.SubmitGenericPartnerApp(lads, new MockUser());
                    }
                }
                ts.Dispose();
            }
        }
    }
}
