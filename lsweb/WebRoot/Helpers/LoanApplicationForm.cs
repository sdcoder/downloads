using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using FirstAgain.Domain.Common;

namespace LightStreamWeb.Helpers
{
    public static class LoanApplicationForm
    {

        public static MvcHtmlString StartForm(string ngOnSuccess)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<form method=\"post\" action=\"#\" name=\"LoanApplication\" class=\"applicationform\" ");

            sb.Append(" ls-validate-on-submit=\"ls-validate-on-submit\" ");
            sb.Append(" error-property=\"LoanApp.ErrorMessage\" ");
            sb.Append($"  on-success=\"{ngOnSuccess}\" >");
            sb.Append("<fieldset id=\"LoanAppFieldset\" >");

            AppendConfirmDeleteOtherIncomeModal(sb);

            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// closes the disabling fieldset, displays the submit button, and closes the form
        /// </summary>
        /// <returns></returns>
        public static MvcHtmlString EndForm(string buttonAttributes = null)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class=\"row\">");
            sb.Append("<div class=\"medium-6 columns\">	");

            sb.Append("<input type=\"submit\" id=\"Continue\" class=\"button\" value=\"Continue\" />");
            sb.Append($"<img src=\"/content/images/ajax-loader.gif\" ng-show=\"Loading\" ng-cloak alt=\"\" {buttonAttributes} />");

            sb.Append("</div>");
            sb.Append("</div>");
            sb.Append("</fieldset>");

            sb.Append("</form>");
            return new MvcHtmlString(sb.ToString());
        }

        /// <summary>
        /// closes the disabling fieldset, displays the submit button, and closes the form
        /// </summary>
        /// <returns></returns>
        public static MvcHtmlString EndFormWithoutButton()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("</fieldset>");

            sb.Append("</form>");
            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString StartFormWithInit(string ngInit, string ngOnSuccess, string @class = "")
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"<form method=\"get\" action=\"#\" name=\"LoanApplication\" class=\"applicationform {@class}\" ");
            sb.Append(" ls-validate-on-submit=\"ls-validate-on-submit\" ");
            sb.Append(" error-property=\"LoanApp.ErrorMessage\" ");
            sb.Append($"  ng-init=\"{ngInit}\" ");
            sb.Append($"  on-success=\"{ngOnSuccess}\" >");
            sb.Append("<fieldset id=\"LoanAppFieldset\" >");


            AppendConfirmDeleteOtherIncomeModal(sb);

            return new MvcHtmlString(sb.ToString());
        }

        private static void AppendConfirmDeleteOtherIncomeModal(StringBuilder sb)
        {
            sb.Append("<div id=\"ConfirmDeleteOtherIncomeDataModal\" class=\"reveal medium\" data-reveal data-options=\"closeOnClick:false\">");
            sb.Append("    <a class=\"close-reveal-modal\" ng-click=\"cancelChangeOtherAnnualIncome()\">&#215;</a>");
            sb.Append("    <h2>Other &quot;Annual&quot; Income Changed</h2>");
            sb.Append("    <p class=\"small-12 columns\">By selecting No, the income data that has been entered will be cleared. Do you still wish to change the Other Annual Income status on this application?</p>");
            sb.Append("    <div class=\"row\">");
            sb.Append("        <div class=\"small-12 columns\">");
            sb.Append("            <a href=\"#\" class=\"button button-medium button-navy\" data-close ng-click=\"cancelChangeOtherAnnualIncome()\">No</a>");
            sb.Append("            <a href=\"#\" class=\"button button-medium\" data-close ng-click=\"confirmChangeOtherAnnualIncome()\">Yes</a>");
            sb.Append("        </div>");
            sb.Append("    </div>");
            sb.Append("</div>");
        }


        public static MvcHtmlString PhoneNumber(string label, string ariaLabel, string ngModel, string nameAndId)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class=\"large-3 medium-4 columns\">");
            sb.Append($"<label for=\"{nameAndId}\"><sup>*</sup>{label}</label>");
            sb.Append($"<input type=\"text\" id=\"{nameAndId}\" name=\"{nameAndId}\" aria-label=\"{ariaLabel}\" ng-model=\"{ngModel}\" ls-phone-number ls-track-visited maxlength=\"14\" required pattern=\"\\d*\" />");

            sb.Append($"<div class=\"formalert\" tabindex=\"0\" ng-show='LoanApplication.{nameAndId}.$hasVisited && !LoanApplication.{nameAndId}.$valid' ng-cloak='' aria-label='Please enter a valid 10 digit phone number.'>");
            sb.Append("<p>{{LoanApplication." + nameAndId + ".$validationMessage}}</p>");
            sb.Append("</div>");
            
            sb.Append("</div>");


            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString SocialSecurityNumber(string label, string ariaLabel, string textAndAriaAlertLabel, string ngModel, string nameAndId, string doesNotMatch = null, string isOnFile = null, string isRequired = "true")
        {
            StringBuilder sb = new StringBuilder();
            
            sb.Append($"<label for=\"{nameAndId}\">");

            if (isRequired.ToLower() == "true")
            {
                sb.Append("<sup>*</sup>");
            }

            sb.Append($"{label}</label>");

            sb.Append($"<input type=\"text\" id=\"{nameAndId}\" name=\"{nameAndId}\" aria-label=\"{ariaLabel}\" ng-model=\"{ngModel}\" ");
            if (doesNotMatch != null)
            {
                sb.Append(" ls-does-not-match=\"" + doesNotMatch + "\" ");
            }
            if (isOnFile != null)
            {
                sb.Append(" ls-is-on-file=\"" + isOnFile + "\" ng-required=\"!(" + isOnFile + ") \" ");
            }
            else if (isRequired.ToLower() == "true")
            {
                sb.Append(" required ");
            }
            sb.Append("maxlength=\"12\" ls-social-security-number ls-track-visited pattern=\"\\d*\" />");

            sb.Append($"<div class=\"formalert\" tabindex=\"0\" ng-cloak='' ng-show='LoanApplication.{nameAndId}.$hasVisited && !LoanApplication.{nameAndId}.$valid && !LoanApplication.{nameAndId}.$error.match' aria-label=\"{textAndAriaAlertLabel}\">");
            sb.Append($"<p>{textAndAriaAlertLabel}</p>");
            sb.Append("</div>");
            sb.Append($"<div class=\"formalert\" tabindex=\"0\" ng-cloak='' ng-show='{ngModel} && !LoanApplication.{nameAndId}.$valid && LoanApplication.{nameAndId}.$error.match'>");
            sb.Append("<p>Applicant and Co-Applicant SSNs must not match</p>");
            sb.Append("</div>");

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString PhoneNumberWithExtension(string label, string ariaLabel, string ariaExtLabel, string ngModel, string nameAndId, string ngRequired = null, string ngDisabled = null)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<div class=\"medium-4 columns\">");
            if (ngRequired != null)
            {
                sb.Append($"<label><sup ng-show=\"{ngRequired}\">*</sup>{label}</label>");
            }
            else
            {
                sb.Append($"<label><sup>*</sup>{label}</label>");
            }
            sb.Append("<div class='row collapse'>");
            sb.Append("<div class='medium-7 columns small-6'>");
            sb.AppendFormat("<input type=\"text\" id=\"{0}\" name=\"{0}\"  ng-model=\"{1}\" ls-phone-number maxlength=\"14\" {2} {3}  pattern=\"\\d*\" ls-track-visited", nameAndId, ngModel,
                            (ngRequired == null) ? "required" : "ng-required=\"" + ngRequired + "\"", (ngDisabled == null) ? "" : "ng-disabled=\"" + ngDisabled + "\"");
            sb.Append($" aria-label=\"{ariaLabel}\"");
            sb.Append($" />");
            sb.Append("</div>");
            sb.Append("<div class='medium-2 columns small-3'>");
            sb.Append("<span class='prefix'>ext</span>");
            sb.Append("</div>");
            sb.Append("<div class='medium-3 columns small-3'>");
            sb.AppendFormat("<input type=\"text\" id=\"{0}\" ng-model=\"{1}\" ls-numbers-only maxlength=\"6\" {2} pattern=\"\\d*\"", nameAndId + "Extenstion", ngModel + ".Extension",
                            (ngDisabled == null) ? "" : "ng-disabled=\"" + ngDisabled + "\"");
            sb.Append($" aria-label=\"{ariaExtLabel}\"");
            sb.Append($" />");
            sb.Append("</div>");
            sb.Append("</div>");

            sb.Append($"<div class=\"formalert\" tabindex=\"0\" ng-cloak ng-show='LoanApplication.{nameAndId}.$hasVisited && !LoanApplication.{nameAndId}.$valid'>");
            sb.Append("<p>{{LoanApplication." + nameAndId + ".$validationMessage}}</p>");
            sb.Append("</div>");
                    
            sb.Append("</div>");
            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString OtherFinancialIncomeField(string label, string ariaLabel, string ngRequired, string name, string error, string hint)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"large-4 medium-7 columns\">");
            sb.Append($"<label><sup ng-show=\"{ngRequired}\">*</sup>{label}</label>");
            sb.AppendLine(@"	</div>");
            sb.AppendLine(@"</div>");
            sb.AppendLine("<div class=\"row\">");
            sb.AppendLine("<div class=\"large-3 medium-4 columns\">");
            sb.Append(LSForm.Currency(nameAndId: name,
                ngModel: string.Format("LoanApp.CombinedFinancials.{0}", name),
                ariaLabel: ariaLabel,
                ngMin: "0",
                ngMax: "99999999.99",
                ngRequired: ngRequired,
                maxLength: 14).ToString());

            sb.Append($"<div ng-cloak tabindex=\"0\" ng-show=\"LoanApplication.{name}.$hasVisited && LoanApplication.{name}.$error.required\" class=\"formalert\">");
            sb.Append($"<p>{error}</p>");
            sb.AppendLine(@"</div>");
            sb.AppendLine(@"</div>");
            sb.AppendLine("<div class='large-9 medium-8 columns'>");
            sb.AppendLine("<div class='forminfo aside' tabindex='0'><p>");
            sb.Append(hint);
            sb.AppendLine(@"</p></div>");
            sb.AppendLine(@"</div>");
            sb.AppendLine(@"</div>");
            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString YearAndMonths(string label, string ariaLabelYear, string ariaLabelMonth, string nameAndId, string ngModel, string ngRequired = null, string ngDisabled = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"<div class='row collapse'>");

            sb.Append($"<label><sup ng-show=\"{ngRequired}\">*</sup>{label}</label>");
            sb.AppendLine(@"<div class='medium-2 columns small-2'>");
            sb.AppendFormat("<input type=\"number\" id=\"{0}\" name=\"{0}\" ", nameAndId + "Years"); 
            sb.Append($"ng-model=\"{ngModel}.Years\" ");
            sb.Append($"aria-label=\"{ariaLabelYear}\" ");
            sb.Append($"ls-numbers-only maxlength=\"2\" ng-min=\"(!{ngModel}.Months || {ngModel}.Months == 0) ? 1 : 0\" ");
            sb.Append($"max=\"99\" ng-required=\"timeFieldIsRequired('{ngRequired}', '{ngModel}.Months')\" ");
            sb.Append($"ng-disabled=\"{ngDisabled}\"> ");
            sb.AppendLine(@"</div>");
            sb.AppendLine("<div class=\"medium-3 columns small-3\"><span class=\"postfix\">Years</span></div>");

            sb.AppendLine("<div class=\"medium-2 columns small-2\">&nbsp;</div>");

            sb.AppendLine("<div class=\"medium-2 columns small-2\">");
            sb.AppendFormat("<input type=\"number\" id=\"{0}\" name=\"{0}\" ", nameAndId + "Months"); 
            sb.Append($"ng-model=\"{ngModel}.Months\" ");
            sb.Append($"aria-label=\"{ariaLabelMonth}\" ");
            sb.AppendLine("ls-track-visited ls-months ls-numbers-only max=\"11\" min=\"0\" maxlength=\"2\" ");
            sb.Append($"ng-required=\"timeFieldIsRequired('{ngRequired}', '{ngModel}.Years')\" ");
            sb.Append($"ng-disabled=\"{ngDisabled}\"> ");
            //sb.Append("{{" + ngModel + ".Years}}");
            //sb.Append("{{LoanApplication." + nameAndId + "Years.$error}}");
            //sb.Append("{{LoanApplication." + nameAndId + "Months.$error}}");
            //sb.Append("{{(" + ngModel + ".Years || " + ngModel + ".Years == 0)}}");
            sb.AppendLine(@"</div>");
            sb.AppendLine("<div class=\"medium-3 columns small-3\"><span class=\"postfix\">Months</span></div>");

            sb.AppendLine(@"</div>");
            return new MvcHtmlString(sb.ToString());

        }
    }
}