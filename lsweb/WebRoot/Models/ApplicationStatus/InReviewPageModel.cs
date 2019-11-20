using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.IDProfile;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public class InReviewPageModel : ApplicationStatusPageModel
    {
        public InReviewPageModel(ICurrentApplicationData currentAppData)
            : base(currentAppData)
        {
            RateModalDisplay = RateModalDisplayType.NotSelected;
        }

        public bool HasBeenCleared()
        {
            var idpads = DomainServiceIDProfileOperations.GetIDProfileAlertsByApplicationId(_currentApplicationData.ApplicationId);
            if (idpads != null)
            {
                return !idpads.IsIDPASet(IdProfileAlertType.ExceptionAfterDecision);
            }

            return false;
        }

    }
}