using System;
using System.Globalization;
using FirstAgain.Common.Data;

namespace LightStreamWeb.Models.AccountServices
{
    public class SelectListOptionPageModel
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public static Func<LookupTable<TLookup>.Value, SelectListOptionPageModel> CreateSelectListOptionFromBindingSource<TLookup>()
        {
            return q => new SelectListOptionPageModel { Text = q.Caption.ToString(CultureInfo.InvariantCulture), Value = q.Enumeration.ToString() };
        }
    }
}