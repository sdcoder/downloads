export var ApplicationTypes;
(function (ApplicationTypes) {
    ApplicationTypes["NotSelected"] = "";
    ApplicationTypes["Individual"] = "Individual";
    ApplicationTypes["Joint"] = "Joint";
})(ApplicationTypes || (ApplicationTypes = {}));
export var PaymentTypes;
(function (PaymentTypes) {
    PaymentTypes["AutoPay"] = "AutoPay";
    PaymentTypes["Invoice"] = "Invoice";
})(PaymentTypes || (PaymentTypes = {}));
export var LoanPurposes;
(function (LoanPurposes) {
    LoanPurposes["HomeImprovement"] = "Home Improvement/Pool/Solar Loan";
    LoanPurposes["EducationalExpenses"] = "PreK-12 Education Loan or Refinance";
    LoanPurposes["Other"] = "Other Loan or Refinance";
    LoanPurposes["BoatRvPurchase"] = "Boat/RV/Aircraft Purchase or Refinance";
    LoanPurposes["MotorcyclePurchase"] = "Motorcycle Purchase or Refinance";
    LoanPurposes["MedicalExpense"] = "Medical/Adoption Expense Loan";
    LoanPurposes["TimeSharePurchase"] = "Timeshare/Fractional Purchase or Refinance";
    LoanPurposes["CreditCardConsolidation"] = "Credit Card/Debt Consolidation Loan";
    LoanPurposes["AutoRefinancing"] = "Auto Loan Refinance";
    LoanPurposes["NewAutoPurchase"] = "New Auto Purchase";
    LoanPurposes["UsedAutoPurchase"] = "Used Auto Purchase from Dealer";
    LoanPurposes["LeaseBuyOut"] = "Auto Lease Buyout";
    LoanPurposes["PrivatePartyPurchase"] = "Used Auto Purchase from Individual";
})(LoanPurposes || (LoanPurposes = {}));
var LoanApplication = /** @class */ (function () {
    function LoanApplication() {
    }
    return LoanApplication;
}());
export { LoanApplication };
var Applicant = /** @class */ (function () {
    function Applicant() {
    }
    return Applicant;
}());
export { Applicant };
var ApplicationType = /** @class */ (function () {
    function ApplicationType(value, text) {
        this.value = value;
        this.text = text;
    }
    return ApplicationType;
}());
export { ApplicationType };
//# sourceMappingURL=loan-application.js.map