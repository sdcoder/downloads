using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using FirstAgain.Domain.SharedTypes.GenericPartner;
using FirstAgain.Domain.Lookups.FirstLook;

/// <summary>
/// Summary description for GenericPartnerHelper
/// </summary>
public static class GenericPartnerHelper
{
	public static GenericPartnerDataSet.GenericPartnerRow GetGenericPartner(this LoanApplicationDataSet lads)
	{
        if (lads.ApplicationDetail[0].PostingPartner.IsNotNull() && lads.ApplicationDetail[0].PostingPartner == PostingPartnerLookup.PostingPartner.Generic)
        {
            var postingPartnerRecord = lads.MarketingSupplementalInfo.FirstOrDefault(m => m.MarketingDataEntity == MarketingDataEntityLookup.MarketingDataEntity.GenericPartnerId);
            if (postingPartnerRecord != null)
            {
                var postingPartner = FirstAgain.Domain.ServiceModel.Client.DomainServiceUtilityOperations.GetGenericPartner(int.Parse(postingPartnerRecord.MarketingDataEntityValue));
                if (postingPartner != null && postingPartner.GenericPartner.Rows.Count == 1)
                {
                    return postingPartner.GenericPartner[0];
                }
            }
        }

        return null;
	}

    public static string GetPartnerDisplayName(this LoanApplicationDataSet lads)
    {
        if (lads.ApplicationDetail[0].PostingPartner.IsNull())
        {
            return string.Empty;
        }

        var partner = lads.GetGenericPartner();
        if (partner != null)
        {
            return partner.DisplayName ?? partner.Name;
        }

        return PostingPartnerLookup.GetCaption(lads.ApplicationDetail[0].PostingPartner);
    }
}