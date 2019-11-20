using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Helpers
{
    public static class MarketingDataSetHelper
    {
        public static bool IsEligibleForDeclineReferral(int? FirstAgainCodeTrackingId)
        {
            bool showOptIn = false;

            if (FirstAgainCodeTrackingId.HasValue)
            {
                var mktds = DomainServiceLoanApplicationOperations.GetCachedMarketingData();

                if (mktds.IsMarketingOrganizationFlagOn(FirstAgainCodeTrackingId.Value, MarketingOrganizationFlagLookup.MarketingOrganizationFlag.EligibleForDeclineReferral))
                {
                    showOptIn = true;
                }
            }
            else
            {
                showOptIn = true;
            }

            return showOptIn;
        }

    }
}