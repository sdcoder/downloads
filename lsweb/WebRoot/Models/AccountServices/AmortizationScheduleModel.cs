using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.AccountServices
{
    [Serializable]
    public class AmortizationScheduleModel
    {
        public List<FundedAccountModel.TransactionHistoryItem> Items { get; set; }
        public int ApplicationId { get; set; }
        [MaxLength(100)]
        public string Format { get; set; }
        public decimal ContractualMonthlyPaymentAmount { get; set; }
        public decimal MinimumMonthlyPaymentAmount { get; set; }
        public DateTime? DecreasedMonthlyPaymentEffectiveDate { get; set; }
        public string JsonItems()
        {
            return JsonConvert.SerializeObject(Items);
        }

    }
}