using FirstAgain.Common.Web;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.UnitTests.Others
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class ScramblingTests
    {
        [Test]
        public void Scramble()
        {
            string plaintext = "firstagain.local,WebUser,StJames2009";
            string encrypted = WebSecurityUtility.EncryptString(plaintext);
            string decrypted= WebSecurityUtility.DecryptString(encrypted);
            Assert.AreEqual(plaintext, decrypted);
        }
    }
}
