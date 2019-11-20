using FirstAgain.Common.Caching;
using FirstAgain.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LightStreamWeb.Controllers
{
    public class UtilityController : ApiController
    {
        // GET api/<controller>
        [Route("api/servertimeinfo")]
        public object Get()
        {
            return new
            {
                Now = DateTime.UtcNow.ToString("s"),
                ExpirationTime = MachineCache.Get<BusinessConstants>("BusinessConstants").FACTHistoryFACTIDExpiration
            };
        }
    }
}