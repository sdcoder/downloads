using FirstAgain.Common.Caching;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.TestingCommon.Builders;
using FirstAgain.Web.Cookie;
using LightStreamWeb.Helpers;
using Moq;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.UnitTests.Helpers
{
    [TestFixture]
    [ExcludeFromCodeCoverage]
    public class MarketingRedirectHelperTest
    {
        [Test]
        public void It_should_not_return_a_redirect_url_when_FACT_is_invalid()
        {
            // Arrange
            var redirectUrl = "not empty url";
            var marketingParams = new MarketingRedirectParameters();
            marketingParams.fact = "NotValidFact";

            // Act
            var redirectHelper = new MarketingRedirectHelper(new Mock<ICookieUtility>().Object);
            var result = redirectHelper.CheckForMarketingRedirect(marketingParams, "", out redirectUrl);

            // Assert
            Assert.AreEqual(string.Empty, redirectUrl);
            Assert.IsFalse(result);
        }

        // TODO: More tests around main check for marketing redirect
         
        public class When_referrer_is_Impact_Radius : MarketingRedirectHelper
        {
            [Test]
            public void It_should_return_marketing_referrer_name_and_FACT_id_when_FACT_is_valid_and_marketing_org_id_is_for_impact_radius()
            {
                // Arrange
                const int fakeFactId = 123;
                const short marketingOrgId = 126;
                const short marketingPlacementId = 14;  // Partner
                const short marketingPlacementCategoryId = 5; // Partner

                var fakeMarketingDataSet = new MarketingDataSetBuilder()
                                            .WithFactRow(fakeFactId, marketingOrgId, marketingPlacementId, marketingPlacementCategoryId)
                                            .Build();

                MachineCache.AddCacheEntry(new MachineCacheEntry("MarketingDataSet", fakeMarketingDataSet));

                // Act
                var result = CheckForImpactRadius(fakeFactId, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

                // Assert

                Assert.AreEqual("ImpactRadius", result.ReferrerName);
                Assert.AreEqual(result.FACT, fakeFactId);
            }

            [Test]
            public void It_should_populate_referrer_info_with_indirect_link_data_when_click_id_is_provided()
            {
                // Arrange
                const int fakeFactId = 123;
                const short marketingOrgId = 126; // Impact Radius
                const short marketingPlacementId = 14;  // Partner
                const short marketingPlacementCategoryId = 5; // Partner
                const string fakeClickId = "XXXXXXXX";

                var fakeMarketingDataSet = new MarketingDataSetBuilder()
                                            .WithFactRow(fakeFactId, marketingOrgId, marketingPlacementId, marketingPlacementCategoryId)
                                            .Build();

                MachineCache.AddCacheEntry(new MachineCacheEntry("MarketingDataSet", fakeMarketingDataSet));

                // Act
                var result = CheckForImpactRadius(fakeFactId, fakeClickId, It.IsAny<string>(), It.IsAny<string>());

                // Assert

                Assert.AreEqual(fakeClickId, result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRClickId.ToString()));
                Assert.AreEqual("IndirectLink", result.GetDataValue("LinkType"));
                Assert.AreEqual("1463", result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRCampaignId.ToString()));
                Assert.AreEqual("3573", result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRActionTrackerId.ToString()));
            }

            [Test]
            public void It_should_populate_referrer_info_with_direct_link_data_when_click_id_is_not_provided()
            {
                // Arrange
                const int fakeFactId = 123;
                const short marketingOrgId = 126; // Impact Radius
                const short marketingPlacementId = 14;  // Partner
                const short marketingPlacementCategoryId = 5; // Partner
              

                var fakeMarketingDataSet = new MarketingDataSetBuilder()
                                            .WithFactRow(fakeFactId, marketingOrgId, marketingPlacementId, marketingPlacementCategoryId)
                                            .Build();

                MachineCache.AddCacheEntry(new MachineCacheEntry("MarketingDataSet", fakeMarketingDataSet));

                // Act
                var result = CheckForImpactRadius(fakeFactId, null, It.IsAny<string>(), It.IsAny<string>());

                // Assert
                Assert.AreEqual(string.Empty, result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRClickId.ToString()));
                Assert.AreEqual("DirectLink", result.GetDataValue("LinkType"));
                Assert.AreEqual("1695", result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRCampaignId.ToString()));
                Assert.AreEqual("3981", result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRActionTrackerId.ToString()));
            }

            [Test]
            public void It_should_populate_referrer_info_with_marketing_partner_id_when_a_valid_one_is_provided()
            {
                // Arrange
                const int fakeFactId = 123;
                const short marketingOrgId = 126; // Impact Radius
                const short marketingPlacementId = 14;  // Partner
                const short marketingPlacementCategoryId = 5; // Partner

                const string expectedMarketingPartnerId = "999";


                var fakeMarketingDataSet = new MarketingDataSetBuilder()
                                            .WithFactRow(fakeFactId, marketingOrgId, marketingPlacementId, marketingPlacementCategoryId)
                                            .Build();

                MachineCache.AddCacheEntry(new MachineCacheEntry("MarketingDataSet", fakeMarketingDataSet));

                // Act
                var result = CheckForImpactRadius(fakeFactId, It.IsAny<string>(), expectedMarketingPartnerId, It.IsAny<string>());

                // Assert
                Assert.AreEqual(expectedMarketingPartnerId, result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRMPID.ToString()));
            }

            [Test]
            public void It_should_populate_referrer_info_with_revised_marketing_partner_id_when_an_invalid_one_is_provided()
            {
                // Arrange
                const int fakeFactId = 123;
                const short marketingOrgId = 126; // Impact Radius
                const short marketingPlacementId = 14;  // Partner
                const short marketingPlacementCategoryId = 5; // Partner

                const string invalidMarketingPartnerId = "999AAA";
                const string revisedMarketingPartnerId = "999";


                var fakeMarketingDataSet = new MarketingDataSetBuilder()
                                            .WithFactRow(fakeFactId, marketingOrgId, marketingPlacementId, marketingPlacementCategoryId)
                                            .Build();

                MachineCache.AddCacheEntry(new MachineCacheEntry("MarketingDataSet", fakeMarketingDataSet));

                // Act
                var result = CheckForImpactRadius(fakeFactId, It.IsAny<string>(), invalidMarketingPartnerId, It.IsAny<string>());

                // Assert
                Assert.AreEqual(revisedMarketingPartnerId, result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRMPID.ToString()));
            }

            [Test]
            public void It_should_populate_referrer_info_with_publisher_id_when_a_valid_one_is_provided()
            {
                // Arrange
                const int fakeFactId = 123;
                const short marketingOrgId = 126; // Impact Radius
                const short marketingPlacementId = 14;  // Partner
                const short marketingPlacementCategoryId = 5; // Partner

                const string expectedPublisherId = "888";


                var fakeMarketingDataSet = new MarketingDataSetBuilder()
                                            .WithFactRow(fakeFactId, marketingOrgId, marketingPlacementId, marketingPlacementCategoryId)
                                            .Build();

                MachineCache.AddCacheEntry(new MachineCacheEntry("MarketingDataSet", fakeMarketingDataSet));

                // Act
                var result = CheckForImpactRadius(fakeFactId, It.IsAny<string>(), It.IsAny<string>(), expectedPublisherId);

                // Assert
                Assert.AreEqual(expectedPublisherId, result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRPID.ToString()));
            }

            [Test]
            public void It_populate_referrer_info_with_revised_publisher_id_when_an_invalid_one_is_provided()
            {
                // Arrange
                const int fakeFactId = 123;
                const short marketingOrgId = 126; // Impact Radius
                const short marketingPlacementId = 14;  // Partner
                const short marketingPlacementCategoryId = 5; // Partner

                const string invalidPublisherId = "888BBB";
                const string expectedPublisherId = "888";

                var fakeMarketingDataSet = new MarketingDataSetBuilder()
                                            .WithFactRow(fakeFactId, marketingOrgId, marketingPlacementId, marketingPlacementCategoryId)
                                            .Build();

                MachineCache.AddCacheEntry(new MachineCacheEntry("MarketingDataSet", fakeMarketingDataSet));

                // Act
                var result = CheckForImpactRadius(fakeFactId, It.IsAny<string>(), It.IsAny<string>(), invalidPublisherId);

                // Assert
                Assert.AreEqual(expectedPublisherId, result.GetDataValue(MarketingDataEntityLookup.MarketingDataEntity.IRPID.ToString()));
            }

        }
    }
}
