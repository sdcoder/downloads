using LightStreamWeb.Models.SignIn;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests
{
    // NOTE: these tests generally require actual logins in the DB, so will fail if you switch to point to different environments
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class SignInTests
    {
        [Test]
        [Ignore("not a unit test")]
        public void QA_Login_NormalAccount()
        {
            var membership = new FirstAgain.Domain.ServiceModel.Client.FirstAgainMembershipProvider();

            Assert.AreEqual(
                SignInModel.LoginResult.UserIdRequired,
                (new SignInModel()
                {
                    UserPassword = "password1"
                }).Login(membership));

            Assert.AreEqual(
                SignInModel.LoginResult.UserPasswordRequired,
                (new SignInModel()
                {
                    UserId = "userid"
                }).Login(membership));

            Assert.AreEqual(
                SignInModel.LoginResult.Success,
                (new SignInModel()
                {
                    UserId = "AstridSecured75",
                    UserPassword = "password1"
                }).Login(membership));
        }

    }
}
