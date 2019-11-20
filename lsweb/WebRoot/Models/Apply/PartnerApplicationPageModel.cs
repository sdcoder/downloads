using System.Collections.Generic;
using System.Linq;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.GenericPartner;
using FirstAgain.Domain.SharedTypes.LoanApplication;
using CMS = FirstAgain.Domain.SharedTypes.ContentManagement;
using System.Diagnostics;
using FirstAgain.Common.Extensions;
using PartnerDisclaimerType = FirstAgain.Domain.Lookups.FirstLook.GenericPartnerDisclaimerTypeLookup.GenericPartnerDisclaimerType;

namespace LightStreamWeb.Models.Apply
{
    public class PartnerApplicationPageModel : BaseInProcessAppPageModel, IBasicRequirementsModel
    {
        public GenericPartnerDataSet.GenericPartnerRow Partner { get; set; }
        public bool HasDisclaimer
        {
            get
            {
                return !string.IsNullOrEmpty(Disclaimer);
            }
        }
        public string Disclaimer { get; private set; }
        public PartnerApplicationPageModel(LoanApplicationDataSet lads) : base(lads)
        {
            if (lads != null)
            {
                Partner = lads.GetGenericPartner();

                if (Partner != null)
                {
                    PopulateDisclaimer();
                }
            }
            BodyClass += " apply";
        }
        public List<CMS.ApplyPage.BasicDynamicRequirement> LoanPurposeBasedBasicRequirements
        {
            get
            {
                return new ContentManager().Get<CMS.ApplyPage>().DynamicRequirements;
            }
        }

        private void PopulateDisclaimer()
        {
            GenericPartnerDataSet.GenericPartnerDisclaimerRow[] disclaimerRows = Partner.GetGenericPartnerDisclaimerRows();
            Disclaimer = string.Empty;

            if (disclaimerRows.Length > 0)
            {
                var values = GenericPartnerDisclaimerTypeLookup.BindingSource.OrderBy(d => d.SortOrder);

                foreach (var value in values)
                {
                    var dr = disclaimerRows.FirstOrDefault(d => d.GenericPartnerDisclaimerType == (PartnerDisclaimerType)value.Enumeration);
                    if (dr.IsNotNull() && dr.GenericPartnerDisclaimerType != PartnerDisclaimerType.NotSelected)
                    {
                        if (dr.GenericPartnerDisclaimerType == PartnerDisclaimerType.SoftPull && !Partner.DoSoftPull) { continue; } // skip if soft pull disclaimer is on, but soft pull is off
                        Disclaimer += string.Format("<p>{0}</p>", dr.DisclaimerText);
                    }
                }
            }
        }

    }
}