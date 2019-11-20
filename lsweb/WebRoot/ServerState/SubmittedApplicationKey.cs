using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.Models.Apply;
using System;

namespace LightStreamWeb.ServerState
{
    [Serializable]
    public class SubmittedApplicationKey
    {
        public SubmittedApplicationKey() { }
        public SubmittedApplicationKey(NativeLoanApplicationModel model, string cookie)
        {
            UserId = model.UserCredentials.UserName;
            Cookie = cookie;
            AppSsn = model.Applicants.Count > 0 ? model.Applicants[0].SocialSecurityNumber : null;
            CoAppSsn = model.Applicants.Count > 1 && model.ApplicationType == ApplicationTypeLookup.ApplicationType.Joint ? model.Applicants[1].SocialSecurityNumber : null;
            LoanAmount = model.LoanAmount;
            LoanTerm = model.LoanTermMonths;
            PurposeOfLoanId = (short)model.PurposeOfLoan.Type;
        }
        public string Cookie { get; set; }
        public string AppSsn { get; set; }
        public string CoAppSsn { get; set; }
        public string UserId { get; set; }
        public decimal LoanAmount { get; set; }
        public short LoanTerm { get; set; }
        public short PurposeOfLoanId { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as SubmittedApplicationKey;
            if (other == null)
                return false;

            return Cookie == other.Cookie &&
                AppSsn == other.AppSsn &&
                CoAppSsn == other.CoAppSsn && 
                UserId == other.UserId &&
                LoanAmount == other.LoanAmount &&
                LoanTerm == other.LoanTerm &&
                PurposeOfLoanId == other.PurposeOfLoanId;
        }

        public override int GetHashCode()
        {
            return (UserId ?? string.Empty).GetHashCode() ^
                 (Cookie ?? string.Empty).GetHashCode() ^
                 (AppSsn ?? string.Empty).GetHashCode() ^
                 (CoAppSsn ?? string.Empty).GetHashCode() ^
                 LoanAmount.GetHashCode() ^
                 LoanTerm.GetHashCode() ^
                 PurposeOfLoanId.GetHashCode();
        }

        public override string ToString()
        {
            return $"{UserId} {Cookie} {AppSsn} {CoAppSsn} {LoanAmount} {LoanTerm} {PurposeOfLoanId}";
        }
    }
}