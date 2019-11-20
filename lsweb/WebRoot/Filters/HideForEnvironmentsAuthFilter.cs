using FirstAgain.Common.Caching;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace LightStreamWeb.Filters
{
    public class HideForEnvironmentsAuthFilter : AuthorizationFilterAttribute
    {
        private readonly List<EnvironmentLookup.Environment> _environments;

        private BusinessConstants _businessConstants => MachineCache.Get<BusinessConstants>("BusinessConstants");

        public HideForEnvironmentsAuthFilter(params EnvironmentLookup.Environment[] environments)
        {
            _environments = new List<EnvironmentLookup.Environment>();

            if (environments != null)
                _environments.AddRange(environments);
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.SkipAuthorization())
                return;

            if (_environments.Any(i => i == _businessConstants.Environment))
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.NotFound);
        }
    }
}