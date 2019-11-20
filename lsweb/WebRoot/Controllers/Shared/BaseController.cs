using FirstAgain.Common.Logging;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.App_State;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace LightStreamWeb.Controllers.Shared
{
    public abstract class BaseController : Controller
    {
        private ICurrentUser _currentUser;

        protected ICurrentUser WebUser
        {
            get
            {
                return _currentUser;
            }
        }

        [Inject]
        public BaseController(ICurrentUser user) 
        {
            _currentUser = user;
        }

        protected void SignOut()
        {
            _currentUser.SignOut();
            FormsAuthentication.SignOut();
        }
    }
}
