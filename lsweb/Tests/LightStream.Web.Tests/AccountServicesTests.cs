using FirstAgain.Correspondence.SharedTypes;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.LoanServicing.ServiceModel.Client;
using FirstAgain.LoanServicing.SharedTypes;
using LightStreamWeb.Models.AccountServices;
using LightStreamWeb.Tests.Mocks;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class AccountServicesTests
    {
        [Test]
        [Ignore("not a unit test")]
        public void LoadData()
        {
            string userName = "serv3test3";

            var response = DomainServiceCustomerOperations.GetAccountInfoByCustomerUserId(userName);
            Assert.IsNotNull(response);
            Assert.IsNotNull(response.ApplicationsDates);
            Assert.IsNotNull(response.CustomerUserIdDataSet);

            BusinessCalendarDataSet businessCalendar = new BusinessCalendarDataSet();
            AccountServicesDataSet accountServicesData = new AccountServicesDataSet();
            DocumentStoreDataSet dds = new DocumentStoreDataSet();

            LoanServicingOperations.GetAccountServicingDataByUserId(userName, out businessCalendar, out accountServicesData, out dds);
            Assert.IsNotNull(businessCalendar);
            Assert.IsNotNull(accountServicesData);

            var model = new AccountServiceModelData(new MockUser());
            model.Populate(response, accountServicesData, businessCalendar);
        }

    }
}
