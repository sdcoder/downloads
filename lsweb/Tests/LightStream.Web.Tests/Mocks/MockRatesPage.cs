using FirstAgain.Domain.SharedTypes.ContentManagement;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests.Mocks
{
    [ExcludeFromCodeCoverage]
    public class MockRatesPage : ICMSRatesPage
    {
        public List<RatesDisclosureContent.LoanTerm> Terms { get; set; }
        public string Header { get; set; }
    }
}
