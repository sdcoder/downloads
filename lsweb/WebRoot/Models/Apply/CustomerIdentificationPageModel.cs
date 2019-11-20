using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Apply
{
    public class CustomerIdentificationPageModel
    {
        public string ApplicantName { get; set; }
        public string InformationRequestUrl { get; set; }
        public ApplicantTypeLookup.ApplicantType ApplicantType { get; set; }
    }
}