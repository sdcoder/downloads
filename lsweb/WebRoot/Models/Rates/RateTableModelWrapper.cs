using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Shared.Rates;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Models.Rates
{
    [ExcludeFromCodeCoverage]
    public class RateTableModelWrapper : RateTableModel
    {
        public RateTableModelWrapper()
        {
            _cmsRatePageContent = new ContentManager().Get<RatesPage>();
        }

        public override void Populate()
        {
            if (ApplicationId.HasValue)
            {
                _rates = DomainServiceInterestRateOperations.GetApplicationInterestRates(ApplicationId.Value);
                ApplicationStatus = _rates.ApplicationInterestRateParams.ApplicationStatus;
            }
            else
            {
                // Use cached rates.
                _rates = DomainServiceInterestRateOperations.GetCachedFixedInterestRates(
                                State.GetValueOrDefault(StateLookup.State.NotSelected),
                                CoApplicantState.GetValueOrDefault(StateLookup.State.NotSelected),
                                PaymentMethod.GetValueOrDefault(PaymentTypeLookup.PaymentType.AutoPay),
                                PurposeOfLoan,
                                FirstAgainCodeTrackingId);
            }

            base.Populate();
        }
    }
}