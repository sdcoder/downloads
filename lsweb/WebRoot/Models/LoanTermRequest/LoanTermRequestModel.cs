using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.LoanTermRequest
{
    public class LoanTermRequestModel
    {
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public bool IsActive { get; set; }
        public int TermMonths { get; set; }
    }
}