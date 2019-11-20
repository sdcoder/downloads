using LightStreamWeb.App_State;
using LightStreamWeb.App_Start;
using NUnit.Framework;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.UnitTests.Mocks;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.UnitTests.Models.Rates
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class RateTableModelTests
    {
        [OneTimeSetUp]
        public static void NinjectSetup()
        {
            NinjectWebCommon.Start();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentUser>().To<MockUser>();
            NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICMSRatesPage>().To<MockRatesPage>();
        }
    }
}
