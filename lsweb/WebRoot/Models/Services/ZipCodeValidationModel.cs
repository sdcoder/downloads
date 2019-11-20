using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirstAgain.Common.Extensions;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace LightStreamWeb.Models.Services
{
    public class ZipCodeValidationModel
    {
        public bool IsValid { get; set; }
        public StateLookup.State State { get; set; }
        public decimal? FloridaDocStampTax { get; set; }
        public decimal? LoanAmount { get; set; }
        [MaxLength(5)]
        public string ZipCode { get; set; }
        [MaxLength(100)] public string City { get; set; }
        public bool IsMilitary { get; set; }

        public void Validate(string zipCode)
        {
            this.ZipCode = zipCode;
            Validate();
        }

        public void Validate()
        {
            if (ZipCode.IsNotNull() && ZipCode.Length == 5)
            {
                var cityState = DomainServiceCustomerOperations.GetZipCodeStateLookup()[ZipCode];
                if (cityState != null)
                {
                    IsValid = true;
                    State = cityState.State;
                    City = cityState.City;

                    if ("AA,AE,AP".Contains(StateLookup.GetCaption(cityState.State)))
                    {
                        IsMilitary = true;
                    }

                    if (cityState.State == StateLookup.State.Florida && LoanAmount.HasValue)
                    {
                        FloridaDocStampTax = FloridaDocumentaryStampTaxCalculator.CalculateTaxWhenTaxIsFinanced(LoanAmount.Value);
                    }
                }
                else
                {
                    if (State == StateLookup.State.Florida && LoanAmount.HasValue)
                    {
                        FloridaDocStampTax = FloridaDocumentaryStampTaxCalculator.CalculateTaxWhenTaxIsFinanced(LoanAmount.Value);
                    }
                }
            }
        }

        public void GetStateTax()
        {
            if (State == StateLookup.State.Florida && LoanAmount.HasValue)
            {
                FloridaDocStampTax = FloridaDocumentaryStampTaxCalculator.CalculateTaxWhenTaxIsFinanced(LoanAmount.Value);
            }
        }

    }
}