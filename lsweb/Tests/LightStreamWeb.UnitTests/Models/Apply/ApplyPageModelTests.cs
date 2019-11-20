using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FirstAgain.Common.Config;
using LightStreamWeb.App_State;
using LightStreamWeb.Models.Apply;
using Moq;
using NUnit.Framework;


namespace LightStreamWeb.UnitTests.Models.Apply
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class ApplyPageModelTests
    {
        [Test]
        public void It_should_not_return_an_impact_radius_js_file_path_when_there_is_no_marketing_referrer()
        {
            // Arrange
            var mockUser = new Mock<ICurrentUser>();
            mockUser.Setup(u => u.MarketingReferrerInfo).Returns((MarketingReferrerInfo)null);

            // Act
            var applyPageModel = new ApplyPageModel(mockUser.Object, new Mock<IThirdPartyRegistrar>().Object, new Mock<ICurrentHttpRequest>().Object);
            var result = applyPageModel.GetImpactRadiusJsFilePath();

            // Assert
            Assert.IsEmpty(result);
        }

        [Test]
        public void It_should_not_return_an_impact_radius_js_file_path_when_not_impact_radius()
        {
            // Arrange
            var mockUser = new Mock<ICurrentUser>();
            mockUser.Setup(u => u.MarketingReferrerInfo).Returns(new MarketingReferrerInfo { ReferrerName = "NotImpactRadius" });

            // Act
            var applyPageModel = new ApplyPageModel(mockUser.Object, new Mock<IThirdPartyRegistrar>().Object, new Mock<ICurrentHttpRequest>().Object);
            var result = applyPageModel.GetImpactRadiusJsFilePath();

            // Assert
            Assert.IsEmpty(result);
        }

        public class When_Marketing_Referrer_Is_Impact_Radius
        {
            [Test]
            public void It_should_return_the_correct_js_file_path_when_is_indirect_link()
            {
                const string impactRadiusKey = "ImpactRadius";
                const string urlKey = "url";
                const string linkTypeKey = "LinkType";

                const string baseUrl = "https://test.d33wwcok8lortz.cloudfront.net";
                const string indirectCampaignId = "1463";
                const string indirectActionTrackerId = "3979";

                var expectedIrJsFilePath = string.Format("{0}/js/{1}/{2}/irv3.js", baseUrl, indirectCampaignId, indirectActionTrackerId);

                var fakeMarketingReferrer = new MarketingReferrerInfo { ReferrerName = impactRadiusKey };
                fakeMarketingReferrer.SetDataNameValue(linkTypeKey, "IndirectLink");

                var mockUser = new Mock<ICurrentUser>();
                mockUser.Setup(u => u.MarketingReferrerInfo).Returns(fakeMarketingReferrer);

                var fakeThirdPartySettingCollection = new ThirdPartySettingCollection
                {
                    Settings = new List<Setting> { new Setting { Name = urlKey, Value = baseUrl } }
                };

                var mockThirdPartyRegistrar = new Mock<IThirdPartyRegistrar>();
                mockThirdPartyRegistrar.Setup(tpr => tpr.GetThirdPartySettings(impactRadiusKey))
                                       .Returns(fakeThirdPartySettingCollection);

                // Act
                var applyPageModel = new ApplyPageModel(mockUser.Object, mockThirdPartyRegistrar.Object, new Mock<ICurrentHttpRequest>().Object);
                var result = applyPageModel.GetImpactRadiusJsFilePath();

                // Assert
                Assert.AreEqual(expectedIrJsFilePath, result);
            }

            [Test]
            public void It_should_return_the_correct_js_file_path_when_is_direct_link()
            {
                const string impactRadiusKey = "ImpactRadius";
                const string urlKey = "url";
                const string linkTypeKey = "LinkType";

                const string baseUrl = "https://test.d33wwcok8lortz.cloudfront.net";
                const string indirectCampaignId = "1695";
                const string indirectActionTrackerId = "3980";

                var expectedIrJsFilePath = string.Format("{0}/js/{1}/{2}/irv3.js", baseUrl, indirectCampaignId, indirectActionTrackerId);

                var fakeMarketingReferrer = new MarketingReferrerInfo { ReferrerName = impactRadiusKey };
                fakeMarketingReferrer.SetDataNameValue(linkTypeKey, "DirectLink");

                var mockUser = new Mock<ICurrentUser>();
                mockUser.Setup(u => u.MarketingReferrerInfo).Returns(fakeMarketingReferrer);

                var fakeThirdPartySettingCollection = new ThirdPartySettingCollection
                {
                    Settings = new List<Setting> { new Setting { Name = urlKey, Value = baseUrl } }
                };

                var mockThirdPartyRegistrar = new Mock<IThirdPartyRegistrar>();
                mockThirdPartyRegistrar.Setup(tpr => tpr.GetThirdPartySettings(impactRadiusKey))
                                       .Returns(fakeThirdPartySettingCollection);

                // Act
                var applyPageModel = new ApplyPageModel(mockUser.Object, mockThirdPartyRegistrar.Object, new Mock<ICurrentHttpRequest>().Object);
                var result = applyPageModel.GetImpactRadiusJsFilePath();

                // Assert
                Assert.AreEqual(expectedIrJsFilePath, result);
            }

            [Test]
            public void It_should_not_return_an_ir_js_file_path_when_there_is_no_impact_radius_url_in_settings()
            {
                // Arrange
                const string impactRadiusKey = "ImpactRadius";
                const string urlKey = "url";

                var fakeMarketingReferrer = new MarketingReferrerInfo { ReferrerName = impactRadiusKey };

                var mockUser = new Mock<ICurrentUser>();
                mockUser.Setup(u => u.MarketingReferrerInfo).Returns(fakeMarketingReferrer);

                var fakeThirdPartySettingCollection = new ThirdPartySettingCollection
                {
                    Settings = new List<Setting> { new Setting { Name = urlKey, Value = string.Empty } }
                };

                var mockThirdPartyRegistrar = new Mock<IThirdPartyRegistrar>();
                mockThirdPartyRegistrar.Setup(tpr => tpr.GetThirdPartySettings(impactRadiusKey))
                                       .Returns(fakeThirdPartySettingCollection);

                // Act
                var applyPageModel = new ApplyPageModel(mockUser.Object, mockThirdPartyRegistrar.Object, new Mock<ICurrentHttpRequest>().Object);
                var result = applyPageModel.GetImpactRadiusJsFilePath();

                // Assert
                Assert.IsEmpty(result);
            }

            [Test]
            public void It_should_clear_cookies()
            {
                // Arrange
                const string impactRadiusKey = "ImpactRadius";
                const string urlKey = "url";

                var mockUser = new Mock<ICurrentUser>();
                mockUser.Setup(u => u.MarketingReferrerInfo).Returns(new MarketingReferrerInfo { ReferrerName = impactRadiusKey });

                var fakeThirdPartySettingCollection = new ThirdPartySettingCollection
                {
                    Settings = new List<Setting> { new Setting { Name = urlKey, Value = string.Empty } }
                };

                var mockThirdPartyRegistrar = new Mock<IThirdPartyRegistrar>();
                mockThirdPartyRegistrar.Setup(tpr => tpr.GetThirdPartySettings(impactRadiusKey))
                                       .Returns(fakeThirdPartySettingCollection);

                // Act
                var applyPageModel = new ApplyPageModel(mockUser.Object, mockThirdPartyRegistrar.Object, new Mock<ICurrentHttpRequest>().Object);
                applyPageModel.GetImpactRadiusJsFilePath();

                // Assert
                mockUser.Verify(u => u.SetMarketingReferrerInfoCookie(null));
            }
        }
    }
}
