using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Web;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Web.Cookie;
using MarketingDataSet = FirstAgain.Domain.SharedTypes.LoanApplication.MarketingDataSet;

namespace LightStreamWeb.Helpers
{
    public interface IFactUtility
    {
        bool IsValidFact(int fact);
    }

    public class FactUtility : IFactUtility
    {
        public FactUtility() { }

        public bool IsValidFact(int fact)
        {
            var marketingData = GetCachedMarketingDataSet();
            var rowsWithFactOnly = marketingData.FirstAgainCodeTrackDetail.Select(String.Format("FirstAgainCodeTrackingId={0}", fact));
            
            return rowsWithFactOnly.Any();
        }

        protected MarketingDataSet GetCachedMarketingDataSet()
        {
           return DomainServiceLoanApplicationOperations.GetCachedMarketingData();
        }
       
    }
}