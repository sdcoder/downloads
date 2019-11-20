using LightStreamWeb.App_State;
using System.Diagnostics.CodeAnalysis;

namespace LightStreamWeb.Tests.Mocks
{
    [ExcludeFromCodeCoverage]
    public class MockHttpRequest : ICurrentHttpRequest
    {
        public MockHttpRequest()
        {
            this.Params = new System.Collections.Specialized.NameValueCollection();
            this.QueryString = new System.Collections.Specialized.NameValueCollection();
        }

        public int Port { get; set; }
        public System.Collections.Specialized.NameValueCollection Params { get; set; }
        public System.Collections.Specialized.NameValueCollection QueryString { get; set; }
        public string RootPath { get; set; }

        public string UrlReferrer { get; set; }

        public string UrlRequested { get; set; }
    }
}
