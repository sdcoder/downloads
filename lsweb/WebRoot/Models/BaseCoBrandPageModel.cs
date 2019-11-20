using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models
{
    public class BaseCoBrandPageModel : BaseLightstreamPageModel
    {
        [ReadOnly(true)]
        public byte[] PartnerLogo { get; protected set; }

    }
}