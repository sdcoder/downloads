using System.Collections.Generic;
using System.Linq;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.Customer;

namespace LightStreamWeb.Models.AccountServices
{
    public class PreferencePageModel
    {
        public string Key { get; set; }
        public bool IsSelected { get; set; }

        public static IList<PreferencePageModel> Populate(SolicitationPreferenceLookup.FilterType filterType, IList<CustomerUserIdDataSet.CustomerSolicitationPreferenceRow> solicitationPreferences)
        {
            var preferenceLookup = solicitationPreferences.ToDictionary(sp => sp.SolicitationPreference, sp => sp.IsPreferred);
            return SolicitationPreferenceLookup.GetFilteredBindingSource(filterType).Select(sp =>
            {
                bool isSelected;
                preferenceLookup.TryGetValue(((SolicitationPreferenceLookup.SolicitationPreference)sp.Enumeration), out isSelected);
                var model = new PreferencePageModel
                {
                    Key = sp.Enumeration.ToString(),
                    IsSelected = isSelected
                };
                return model;
            }).ToList();
        }
    }
}