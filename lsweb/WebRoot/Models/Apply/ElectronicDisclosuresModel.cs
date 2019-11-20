using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Apply
{
    public class ElectronicDisclosuresModel
    {
        [Required]
        public bool ElectronicDisclosures { get; set; }
    }
}