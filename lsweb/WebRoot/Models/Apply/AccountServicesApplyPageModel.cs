using FirstAgain.Common.Web;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.App_State;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Apply
{
    public class AccountServicesApplyPageModel : ApplyPageModel
    {
        public AccountServicesApplyPageModel(ICurrentUser user)
            : base(user)
        {
            BodyClass = "apply account-services";
        }

        public int ActiveApplicationId
        {
            get
            {
                return WebUser.ApplicationId.GetValueOrDefault();
            }
        }

        public string Ctx
        { 
            get
            {
                return WebSecurityUtility.Scramble(ActiveApplicationId); 
            }
        }

        public string AccountServicesIntro
        {
            get
            {
                return new ContentManager().Get<ApplyPage>().AccountServicesIntro;
            }
        }

        public bool HasFundedAccount => WebUser.IsAccountServices;
    }
}