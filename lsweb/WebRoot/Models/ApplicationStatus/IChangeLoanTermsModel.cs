using LightStreamWeb.Models.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public interface IChangeLoanTermsModel
    {
        LoanTermsModel LoanTerms { get; }
        string ChangeLoanTermsMessage { get;  }
        bool PurposeOfLoanIsSecured();
        string CdnBaseUrl { get; }
    }
}
