using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using System.ComponentModel.DataAnnotations;

namespace LightStreamWeb.Models.Marketing
{
    public class SubscriptionModel
    {
        [MaxLength(50)]
        public string FirstName { get; set; }
        [MaxLength(50)]
        public string LastName { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public ProductInterest ProductInterest { get; set; }

        public bool PostSubscriber()
        {
            return DomainServiceUtilityOperations.AddMarketingSubscriber(FirstName, LastName, Email, ProductInterest.ToLookup());
        }
    }

    public class ProductInterest
    {
        [MaxLength(50)]
        public string Caption { get; set; }
        public int Id { get; set; }
        [MaxLength(100)]
        public string Label { get; set; }


        public PurposeOfLoanLookup.PurposeOfLoan ToLookup()
        {
            return (PurposeOfLoanLookup.PurposeOfLoan)Id;
        }
    }

}