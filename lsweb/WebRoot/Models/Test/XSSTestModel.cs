using FirstAgain.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Test
{
    public class XSSTestModel
    {
        public XSSTestModel()
        {
            ReadWrite = "I'm <b>Read Only</b>";
            PublicReadPrivateSet = "I'm <b>Private</b>";
            Statements1 = new List<string>() { "a", "b", "c" };
            Statements2 = new List<string>() { "a", "b", "c" };
            Statements3 = new List<string>() { "a", "b", "c" };
            _readOnly = "I'm <b>Read Only</b>";
        }
        public static string Static = "<b>I'm Static</b>";

        public List<string> Statements1 { get; set; }
        public List<string> Statements2 { get; }
        public List<string> Statements3 { get; private set; }

        private string _readOnly;
        public string ReadOnly
        {
            get
            {
                return _readOnly;
            }
        }
        public string ReadOnlyToBusinessConstant
        {
            get
            {
                return BusinessConstants.Instance.ACHRoutingNumberUrl;
            }
        }

        public string MethodCallToPrivateVariable()
        {
            return _readOnly;
        }

        public string MethodCallToService()
        {
            return FirstAgain.Domain.ServiceModel.Client.DomainServiceUtilityOperations.GetAllGenericPartners().GenericPartner.First().DisplayName;
        }

        public string PublicReadPrivateSet { get;  private set; }

        public string ReadWrite { get; set; }
        public string Dynamic { get; set; }
    }
}