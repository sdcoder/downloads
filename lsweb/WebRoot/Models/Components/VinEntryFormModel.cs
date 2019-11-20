using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Components
{
    public class VinEntryFormModel
    {
        public List<string> ApplicantNames { get; set; }
        public string       LoanNumber { get; set; }
        public decimal      RequestedLoanAmount { get; set; }
        public bool         IsPreFunding { get; set; }
        public string       VIN { get; set; }
        public decimal?     Mileage { get; set; }
        public decimal?     ProceedsPaidToDealer { get; set; }
        public string       Description { get; set; }
        public string       Year { get; set; }
    }
}