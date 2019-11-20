using FirstAgain.Domain.ServiceModel.Contracts.MessageContracts.CustomerOperations;
using LightStreamWeb.ServerState;
using LightStreamWeb.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.App_State
{
    public static class CurrentUserExtensions
    {
        public static GetAccountInfoResponse Refresh(this ICurrentUser webUser)
        {
            if (webUser.ApplicationId.HasValue)
            {
                var accountInfo = SessionUtility.RefreshAccountInfo();
                var loanOfferDataSet = FirstAgain.Domain.ServiceModel.Client.DomainServiceLoanApplicationOperations.GetLoanOffer(webUser.ApplicationId.Value);
                SessionUtility.CleanUpSessionForRefresh();
                SessionUtility.SetLoanOfferDataSet(loanOfferDataSet);
                SessionUtility.ReloadApplicationData(webUser.ApplicationId.Value);
                SessionUtility.SetCurrentApplicationData(webUser.ApplicationId.Value, DataSetToSessionStateMapper.Map(webUser.ApplicationId.Value, accountInfo.CustomerUserIdDataSet, loanOfferDataSet));
                return accountInfo;
            }

            return null;
        }
    }
}