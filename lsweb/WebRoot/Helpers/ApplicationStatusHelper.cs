using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Helpers
{
    internal static class ApplicationStatusHelper
    {
        public static List<SelectListItem> PopulateAuthorizedSignerList(FirstAgain.Domain.SharedTypes.Customer.CustomerUserIdDataSet.ApplicationRow application)
        {
            var AuthorizedSignerList = new List<SelectListItem>();

            if (application.IsJoint)
            {
                AuthorizedSignerList.Add(new SelectListItem() { Value = BankAccountHolderNameTypeLookup.BankAccountHolderNameType.PrimaryBorrowerName.ToString(), Text = application.PrimaryApplicant.FullName });
                AuthorizedSignerList.Add(new SelectListItem() { Value = BankAccountHolderNameTypeLookup.BankAccountHolderNameType.SecondaryBorrowerName.ToString(), Text = application.SecondaryApplicant.FullName });

                string both = application.PrimaryApplicant.FullName + " and " + application.SecondaryApplicant.FullName;
                AuthorizedSignerList.Add(new SelectListItem() { Value = BankAccountHolderNameTypeLookup.BankAccountHolderNameType.BothBorrowersNames.ToString(), Text = both });
            }
            else
            {
                AuthorizedSignerList.Add(new SelectListItem() { Value = BankAccountHolderNameTypeLookup.BankAccountHolderNameType.PrimaryBorrowerName.ToString(), Text = application.PrimaryApplicant.Name });
                AuthorizedSignerList.First().Selected = true;
            }

            return AuthorizedSignerList;
        }



    }
}