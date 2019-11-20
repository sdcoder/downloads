using LightStreamWeb.App_State;
using LightStreamWeb.UnitTests.Mocks;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.UnitTests.Models.ApplicationStatus
{
    [SetUpFixture]
    [ExcludeFromCodeCoverage]
    public class NUnitSetup
    {
        [OneTimeSetUp]
        public static void SetUp()
        {
            App_Start.NinjectWebCommon.Start();
            App_Start.NinjectWebCommon.Bootstrapper.Kernel.Rebind<ICurrentUser>().To<MockUser>();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            App_Start.NinjectWebCommon.Stop();
        }

    }
}
