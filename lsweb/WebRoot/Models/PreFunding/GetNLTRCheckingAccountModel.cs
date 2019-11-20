using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.LoanServicing.SharedTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.PreFunding
{
    [Serializable]
    public class GetNLTRCheckingAccountModel
    {
        public GetNLTRCheckingAccountModel()
        {
            EmailPreferences = new List<SolicitationPreferenceLookup.SolicitationPreference>();
            AuthorizedSigner = BankAccountHolderNameTypeLookup.BankAccountHolderNameType.PrimaryBorrowerName;
        }

        public List<SolicitationPreferenceLookup.SolicitationPreference> EmailPreferences { get; set; }

        public BankAccountHolderNameTypeLookup.BankAccountHolderNameType AuthorizedSigner { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public AccountActionType AccountAction { get; set; }

        public enum AccountActionType
        {
            SameAsChecking,
            SameAsFunding,
            DifferentAccount
        }

    }
}