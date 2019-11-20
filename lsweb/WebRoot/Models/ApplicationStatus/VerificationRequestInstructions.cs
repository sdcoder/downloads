using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using VerificationType = FirstAgain.Domain.Lookups.FirstLook.VerificationTypeLookup.VerificationType;

namespace LightStreamWeb.Models.ApplicationStatus
{
    public static class VerificationRequestInstructions
    {

        private static string GovernmentIssuedPhotoIdInformation
        {
            get
            {
                return "Must be a driver's license, passport, or state issued ID card. Cannot be a military/government employee ID.";
            }
        }

        private static Dictionary<VerificationType, string> Instructions
        {
            get
            {
                return new Dictionary<VerificationType, string>
                {
                    { VerificationType.BankruptcyDocumentation, "Provide your official Bankruptcy Notice so that we may update your account." },
                    { VerificationType.BankStatementSecuredAuto, "Provide a bank statement showing that your payoff has cleared so we may expedite the title release." },
                    { VerificationType.BankingOrBrokerage, "Online or paper statements must be dated within 30 days or within the most recent quarter, show your full name, the name of the financial institution, and include all pages and transaction details." },
                    { VerificationType.BillOfSaleSecuredAuto, "Provide a Bill of Sale or Purchase Agreement from the dealership that your vehicle was purchased from." },
                    { VerificationType.BonusIncome, "W-2s must be for the most recent tax year, show your full name and your employer's name. Pay stubs must be dated within 30 days, show your full name, employer's name, earnings and deductions breakdowns." },
                    { VerificationType.BorrowersAuthorization, "Provide the authorization form granting permission to release information to a third party." },
                    { VerificationType.DriverLicense, GovernmentIssuedPhotoIdInformation },
                    { VerificationType.Check, "Provide a full copy of a voided check showing your name and bank account number." },
                    { VerificationType.CreditCardStatement, "Provide a copy of the requested credit card statement showing your full name, address and transaction history." },
                    { VerificationType.CreditCardStatementPOP, "Provide proof of the use of your loan proceeds by providing a credit card and/or lender statement." },
                    { VerificationType.CustomerExperienceGuaranteeSurvey, "Upload your completed Customer Experience Guarantee Survey." },
                    { VerificationType.EmploymentOffer, "Provide a signed copy of your employment offer letter showing your name, employer's name, expected salary, and first date of employment." },
                    { VerificationType.FederalTaxReturn, "Provide a complete copy of your most recently filed and signed Federal Tax Return form 1040, including all applicable schedules (1, C, D, E, F, K1, etc.)." },
                    { VerificationType.FederalTaxReturn2Yr, "Provide complete copies of your filed and signed Federal Tax Return form 1040 for the last two years, with all applicable schedules (1, C, D, E, F, K1, etc.)." },
                    { VerificationType.GovernmentIssuedPhotoID, GovernmentIssuedPhotoIdInformation },
                    { VerificationType.IRA, "Online or paper statements must be dated within 30 days or within the most recent quarter, show your full name, the name of the financial institution, and include all pages and transaction details." },
                    { VerificationType.IRS4506T, "To access IRS Form 4506-T follow this <a href='http://www.irs.gov/pub/irs-pdf/f4506t.pdf' data-jump=\"true\" target=\"_new\">link</a>." },
                    { VerificationType.LeinReleaseLetter, "Provide a lien release letter on your previous lender's letterhead. Please note that we require the letter to reflect the vehicle’s VIN." },
                    { VerificationType.NameChange, "Provide your official Name Change Document." },
                    { VerificationType.PayStub, "Provide a complete pay stub, dated within 30 days, showing your full name, employer's name, earnings and deductions breakdowns." },
                    { VerificationType.PhoneBill, "The phone bill must be dated within 30 days, show your full name, service address, and utility company's name." },
                    { VerificationType.ProofOfLienUCCFilingReceipt, "Provide proof that LightStream has been added as lienholder for your vehicle. This may be in the form of a security interest filing document, a lienholder receipt, a UCC filing receipt, or a copy of your vehicle's registration." },
                    { VerificationType.ProofOfUseOfProceeds, "Provide proof that the loan proceeds were used for the purpose indicated on your loan application. Some examples of acceptable proof are receipts, statements, or contract documents." },
                    { VerificationType.SCRAOrders, "Provide Military Orders or a signed letter from your commanding officer that includes dates of active duty." },
                    { VerificationType.SignedPage10LoanAgreement, "In order for us to process our lien, the state needs a physical signature. Please print, sign, and email page 10 of your loan agreement. You may locate this page by selecting the \"Documents\" tab." },
                    { VerificationType.TitleImageSecuredAuto, "Provide a copy of the vehicle's title to verify that it is lien-free." },
                    { VerificationType.UtilityBill, "Utility bill must be for a major service (i.e. home phone/cable/internet, gas or electric). The utility bill must be dated within 30 days, show your full name, service address, and utility company's name." },
                    { VerificationType.W2, "Provide a full copy of your most recent W-2 showing your full name, employer's name, earnings and deductions breakdowns." }
                };
            }
        }

        public static string GetInstruction(VerificationType verificationType)
        {
            string instruction = string.Empty;

            Instructions.TryGetValue(verificationType, out instruction);

            return instruction;
        }
    }
}