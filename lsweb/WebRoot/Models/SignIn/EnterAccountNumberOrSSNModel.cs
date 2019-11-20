using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.SignIn
{
    public class EnterAccountNumberOrSSNModel
    {
        private string _socialSecurityNumber = string.Empty;
        
        public enum RecoveryType
        {
            SocialSecurityNumber,
            AccountNumber
        }

        public EnterAccountNumberOrSSNModel(){}

        [MaxLength(12)]
        public string SocialSecurityNumber
        {
            get
            {
                return _socialSecurityNumber;
            }
            set
            {
                _socialSecurityNumber = value.Trim().Replace("-","");
            }
        }
        public int AccountNumber { get; set; }
        public RecoveryType SelectedType { get; set; }
    }
}