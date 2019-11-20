using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Ninject;

using FirstAgain.Common.Extensions;
using FirstAgain.Domain.SharedTypes.Customer;
using LightStreamWeb.ServerState;
using FirstAgain.Web.UI;

using LightStreamWeb.App_State;

namespace LightStreamWeb.Controllers.Shared
{
    public class BaseAccountController : BaseController
    {
        [Inject]
        public BaseAccountController(ICurrentUser user)
            : base(user) {}

        public CustomerUserIdDataSet Account
        {
            get
            {
                return SessionUtility.CustomerUserIdDataSet;
            }
        }

        public CustomerUserIdDataSet.ApplicationRow Application
        {
            get
            {
                return Account.IsNull() ? null : Account.Application.FindByApplicationId(new CurrentUser().ApplicationId.GetValueOrDefault());
            }
        }
    }
}
