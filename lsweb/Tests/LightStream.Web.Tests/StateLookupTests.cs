using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.Models.Services;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class StateLookupTests
    {
        [Test]
        [Ignore("not a unit test")]
        public void Test_InvalidZip()
        {
            ZipCodeValidationModel model = new ZipCodeValidationModel();

            model.Validate("A");
            Assert.IsFalse(model.IsValid);

            model.Validate("00000");
            Assert.IsFalse(model.IsValid);

            model.Validate("92101");
            Assert.IsTrue(model.IsValid);
            Assert.AreEqual(StateLookup.State.California, model.State);

        }
    }
}
