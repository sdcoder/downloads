using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Funding
{
    [Serializable]
    public class BrokerageAccountModel
    {
        public bool IsBrokerageAccount { get; set; }
        public bool IsBrokerageWireAccount { get; set; }
        public string BeneficiaryBankName { get; set; }
        public string BeneficiaryABANumber { get; set; }
        public string BeneficiaryAccountNumber { get; set; }
        public string IntermediaryBankName { get; set; }
        public string IntermediaryBankABANumber { get; set; }
        public string IntermediaryBankAccountNumber { get; set; }
    }
}