using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Test
{
    public class LendingTreePostConfiguration
    {
        public LendingTreePostConfiguration(EnvironmentLookup.Environment environment)
        {
            if (environment == EnvironmentLookup.Environment.Production)
            {
                Username = "lendingtree";
                Password = "lendLT?1";
            }
        }

        public string Username { get; private set; }
        public string Password { get; private set; }
    }
}