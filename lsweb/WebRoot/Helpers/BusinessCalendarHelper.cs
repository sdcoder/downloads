using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Helpers
{
    public class BusinessCalendarHelper
    {
        public static bool CanFundToday()
        {
            //check if today is a banking date 
            var DateTimeNow = DateTime.Now;
            var nextBankingdate = FirstAgain.Domain.ServiceModel.Client.BusinessCalendar.GetClosestBankingDay(DateTimeNow);

            if (nextBankingdate.Date != DateTimeNow.Date)
            {
                return false;
            } 

            //check custoff time
            DateTime cutoff = new DateTime(DateTime.Now.Year,
                DateTime.Now.Month,
                DateTime.Now.Day,
                BusinessConstants.Instance.SameDayFundingCutoffTimeWeb.Hours,
                BusinessConstants.Instance.SameDayFundingCutoffTimeWeb.Minutes,
                0);
            return DateTimeNow < cutoff;
        }

        public static System.Collections.Hashtable GetCalendarFundingDates(int applicationId)
        {
            var fundingDates = FirstAgain.Domain.ServiceModel.Client.BusinessCalendar.GetPossibleFundingDates(applicationId, true);
            var CalendarFundingDates = new System.Collections.Hashtable();
            foreach (var d in fundingDates)
            {
                if (d.FundsTransferType != FundsTransferTypeLookup.FundsTransferType.NotSelected)
                {
                    CalendarFundingDates[d.Date.ToString("yyyy-M-d")] = (d.FundsTransferType == FundsTransferTypeLookup.FundsTransferType.ACHTransaction) ? "ACH" : "Wire";
                }
            }

            return CalendarFundingDates;
        }
    }
}