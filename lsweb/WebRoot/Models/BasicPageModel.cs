using LightStreamWeb.App_State;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models
{
    public class BasicPageModel : BaseLightstreamPageModel
    {
        public BasicPageModel() : base()
        {
            
        }

        [Inject]
        public BasicPageModel(ICurrentUser user)
            : base(user)
        {
            
        }

    }
}