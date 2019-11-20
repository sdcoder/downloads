using System.Collections.Generic;
using System.Linq;
using FirstAgain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;

namespace LightStreamWeb.Models.AccountServices
{
    public class ApplicantPageModel
    {
        public int ApplicantId { get; private set; }
        public ContactInformationPageModel ContactInformation { get; private set; }
        public IList<PreferencePageModel> PrivacyPreferences { get; set; }
        public IList<PreferencePageModel> EmailPreferences { get; set; }

        public static ApplicantPageModel Populate(AccountContactInfo.AccountInfo applicantInfo, IList<CustomerUserIdDataSet.CustomerSolicitationPreferenceRow> applicantEmailPrivacyPreferences)
        {
            Guard.AgainstNull(applicantInfo, "applicantInfo");
            var emailPreferences = PreferencePageModel.Populate(SolicitationPreferenceLookup.FilterType.Customer, applicantEmailPrivacyPreferences) as List<PreferencePageModel>;
            emailPreferences.RemoveAll(i => i.Key == emailPreferences.FirstOrDefault(x => x.Key == "ParticipateInEmailSurveys")?.Key);
            emailPreferences.RemoveAll(i => i.Key == emailPreferences.FirstOrDefault(x => x.Key == "ParticipateInTestimonialPgm")?.Key);

            return new ApplicantPageModel
            {
                ApplicantId = applicantInfo.ApplicantId,
                ContactInformation = ContactInformationPageModel.Populate(applicantInfo),
                PrivacyPreferences = PreferencePageModel.Populate(SolicitationPreferenceLookup.FilterType.Privacy, applicantEmailPrivacyPreferences),
                EmailPreferences = emailPreferences
            };
        }
    }
}