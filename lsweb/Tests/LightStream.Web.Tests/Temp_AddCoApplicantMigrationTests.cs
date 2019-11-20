using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_Start;
using LightStreamWeb.App_State;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.ApplicationStatus;
using LightStreamWeb.Models.SignIn;
using LightStreamWeb.Tests.Mocks;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class Temp_AddCoApplicantMigrationTests
    {
        [OneTimeSetUp]
        public static void NinjectSetup()
        {
            NinjectWebCommon.Start();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentHttpRequest>().To<MockHttpRequest>();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentUser>().To<MockUser>();
        }

        //julyx20012

        [Test]
        [Ignore("not a unit test")]
        public void TestAllTheThings()
        {
            foreach (ApplicationStatusTypeLookup.ApplicationStatusType applicationStatus in Enum.GetValues(typeof(ApplicationStatusTypeLookup.ApplicationStatusType)))
            {
                bool truncated;
                var results = DomainServiceLoanApplicationOperations.SearchForLoanApplications(new FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationSearchCriteria()
                {
                    ApplicationStatusSearchCriteria = new FirstAgain.Domain.SharedTypes.LoanApplication.ApplicationStatusSearchCriteria()
                    {
                        ApplicationStatus = applicationStatus.ToString()
                    }
                }, out truncated);

                foreach (var result in results)
                {
                    var customerUserId = result.CustomerUserId;
                    AssertDataMappingForAccount(customerUserId);
                }

            }
            Assert.True(true);
            Trace.Write("DONE");
        }

        [Test]
        [Ignore("not a unit test")]
        public void OneScenarioOrTwo()
        {
            // 11134725 
            AssertDataMappingForAccount("apr14y124120");
//            AssertAddCoApplicantUseCases(ApplicationStatusTypeLookup.ApplicationStatusType.PendingQ, "creditpolicytest7056");
        }



        private static void AssertDataMappingForAccount(string customerUserId)
        {
            var accountData = new SignInModel().GetAccountStatus(customerUserId);
            
            foreach (var ad in accountData.ApplicationsDates.ApplicationsDates)
            {

                var loanOfferDataSet = DomainServiceLoanApplicationOperations.GetLoanOffer(ad.ApplicationId);
                var oldModel = new TestApplicationStatusPageModel(ad.ApplicationId, accountData.AccountInfo, loanOfferDataSet);

                var sessionData = DataSetToSessionStateMapper.Map(ad.ApplicationId, accountData.AccountInfo, loanOfferDataSet);
                var newModel = new ApplicationStatusPageModel(sessionData);

                Trace.Write($"{ad.ApplicationId} {customerUserId} {oldModel.CurrentStatus}");

                Assert.AreEqual(ad.ApplicationId, newModel.ApplicationId);
                Assert.AreEqual(oldModel.HasDeclineNotice(), newModel.HasDeclineNotice());
                Assert.AreEqual(oldModel.HasEnotices(), newModel.HasEnotices());
                Assert.AreEqual(oldModel.ApplicationResultedFromAddCoApplicant(), newModel.ApplicationResultedFromAddCoApplicant());
                Assert.AreEqual(oldModel.AddCoApplicantIsEnabled(), newModel.AddCoApplicantIsEnabled());

                Assert.AreEqual(oldModel.GetSolicitationPreferenceType(), newModel.GetSolicitationPreferenceType());
                Assert.AreEqual(oldModel.IsPrime5, newModel.IsPrime5);
                Assert.AreEqual(oldModel.IsPrime6, newModel.IsPrime6);

                Assert.AreEqual(oldModel.GetPurposeOfLoan(), newModel.GetPurposeOfLoan());
                Assert.AreEqual(oldModel.CurrentStatus, newModel.CurrentStatus);

                Assert.AreEqual(oldModel.ApplicationType, newModel.ApplicationType);

            }
        }

        class TestApplicationStatusPageModel : ApplicationStatusPageModel
        {
            public TestApplicationStatusPageModel(int applicationId, GetAccountInfoResponse accountInfo, LoanOfferDataSet loanOfferDataSet) : base(accountInfo, loanOfferDataSet)
            {
                _user.ApplicationId = applicationId;
            }
        }
    }
}
