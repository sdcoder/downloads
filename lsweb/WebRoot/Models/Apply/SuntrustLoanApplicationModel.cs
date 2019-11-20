using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

using FirstAgain.Domain.SharedTypes.LoanApplication.LoanApplicationPostData;
using FirstAgain.Domain.Lookups.FirstLook;

namespace LightStreamWeb.Models.Apply
{
    [Serializable]
    [XmlRoot("SuntrustLoanApplication")]
    public class SuntrustLoanApplicationModel : SuntrustLoanApplicationPostData
    {
        public string HowYouHeardAboutUs
        {
            get
            {
                MarketingEntityDataPostData mdedata = MarketingEntityData.Where(mde => mde.MarketingEntity == MarketingDataEntityLookup.MarketingDataEntity.SubId).FirstOrDefault();
                return mdedata != null ? mdedata.EntityValue : string.Empty;
            }
            set
            {
                MarketingEntityDataPostData mdedata = MarketingEntityData.Where(mde => mde.MarketingEntity == MarketingDataEntityLookup.MarketingDataEntity.SubId).FirstOrDefault();
                if (mdedata == null && !string.IsNullOrEmpty(value))
                {
                    mdedata = new MarketingEntityDataPostData() { MarketingEntity = MarketingDataEntityLookup.MarketingDataEntity.SubId, EntityValue = value };
                    MarketingEntityData.Add(mdedata);
                }
                else if (mdedata != null && !string.IsNullOrEmpty(value))
                {
                    mdedata.EntityValue = value;
                }
 
            }
        }
    }
}