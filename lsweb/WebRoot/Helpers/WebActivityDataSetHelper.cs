using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirstAgain.Common.Extensions;

namespace LightStreamWeb.Helpers
{
    internal static class WebActivityDataSetHelper
    {
        public static WebActivityDataSet Populate(ICurrentUser user, ApplicationStatusTypeLookup.ApplicationStatusType applicationStatus = ApplicationStatusTypeLookup.ApplicationStatusType.InProcess)
        {
            WebActivityDataSet wads = new WebActivityDataSet();

            WebActivityDataSet.WebActivityRow row = wads.WebActivity.NewWebActivityRow();
            row.Cookie = user.UniqueCookie;
            row.IPAddress = new LightStreamWeb.App_State.CurrentUser().IPAddress;
            row.ApplicationStatusTypeId = (short)applicationStatus;
            row.UserAgent = user.UserAgent.Truncate(255) ?? string.Empty ;
            row.AcceptLanguage = user.AcceptLanguage.Truncate(255) ?? string.Empty;
            row.EventAuditLogId = 0;
            // backend will seed this after creating the eventauditlog record assuming this ds is passed in to one of the Submit[Foo]WithWebActivity() methods
            wads.WebActivity.AddWebActivityRow(row);

            return wads;
        }

    }
}