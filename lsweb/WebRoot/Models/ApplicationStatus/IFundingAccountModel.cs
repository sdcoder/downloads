using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public interface IFundingAccountModel
    {
        List<SelectListItem> AuthorizedSignerList { get; }
        ApplicationTypeLookup.ApplicationType ApplicationType { get; }
        bool IsJoint { get; }
    }
}
