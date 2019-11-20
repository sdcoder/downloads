using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Helpers
{
    /// <summary>
    /// LSForm
    /// 
    /// Helper methods for displaying input fields with all the correct attributes for validation implemented by the angular.js.forms client-size script.
    /// </summary>
    public static class LSForm
    {

        public const string NamePattern = @"^[a-zA-Z][A-Za-z\' \.]*$";

        public static List<SelectListItem> GetPaymentDatesList(bool selectNull = false)
        {
            var result = new List<SelectListItem>();
            if (selectNull)
            {
                result.Add(new SelectListItem() { Selected = true });
            }
            for (var num = 1; num <= 28; num++)
            {
                string display = GetPaymentDate(num);
                result.Add(new SelectListItem() { Value = num.ToString(), Text = display });
            }
            result.Add(new SelectListItem() { Value = "99", Text = "Last day of the month" });
            return result;
        }

        public static string GetPaymentDate(int num)
        {
            string display = num.ToString();

            if (num >= 10 && num <= 20)
            {
                display = num + "th";
            }
            else
            {
                switch (num % 10)
                {
                    case 1:
                        display = num + "st";
                        break;
                    case 2:
                        display = num + "nd";
                        break;
                    case 3:
                        display = num + "rd";
                        break;
                    default:
                        display = num + "th";
                        break;
                }
            }

            return display;
        }
        
        public static MvcHtmlString DriversLicenseLastFour(string nameAndId, string ariaLabel, string ngModel)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("<input type=\"text\" ");
            sb.AppendFormat("id=\"{0}\" ls-proper-case=\"upper\" ng-model=\"{1}\" aria-label=\"{2}\" ", nameAndId, ngModel, ariaLabel);
            sb.Append("ng-minlength=\"4\" maxlength=\"4\"  required ng-pattern=\"/^[A-Za-z0-9]+$/\" ls-restrict-to-pattern > ");
            return new MvcHtmlString(sb.ToString());
        }

        // ls-zip-code
        public static MvcHtmlString ZipCode(string nameAndId, string ngModel, string ariaLabel, bool required = false, string ngRequired = null, string ngDisabled = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<input type=\"text\" ls-zip-code name=\"{0}\" id=\"{0}\" aria-label=\"{1}\" ng-model=\"{2}\" ", nameAndId, ariaLabel, ngModel);
            if (required) 
            {
                sb.Append("required=\"required\" ");
            }
            if (ngRequired != null)
            {
                sb.AppendFormat("ng-required=\"{0}\" " , ngRequired);
            }
            if (ngDisabled != null)
            {
                sb.AppendFormat("ng-disabled=\"{0}\" " , ngDisabled);
            }
            sb.Append(" pattern=\"\\d*\" >");
            return new MvcHtmlString(sb.ToString());
        }

        // ls-bank-account-number, but allows leading zero's
        public static MvcHtmlString BankAccountNumber(string nameAndId, string ngModel, bool required = true, string attributes = null, string formName = null)
        {
            var minLength = 2;
            var maxLength = 17;
            var regExPattern = "/^[a-zA-Z0-9-]+$/";
            var errorMessage = "Please enter your Account Number without special characters or periods.";

            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<input type=\"text\" ls-disallow-spaces ng-pattern=\"{0}\" ls-track-visited name=\"{1}\" id=\"{1}\" ng-model=\"{2}\" ng-required=\"{3}\" ", regExPattern, nameAndId, ngModel, required);
            if (required)
            {
                sb.Append("required=\"required\" ");
            }
            sb.AppendFormat("min=\"{0}\" ", minLength);
            sb.AppendFormat("max=\"{0}\" ", maxLength);
            sb.AppendFormat("ng-minlength=\"{0}\" ", minLength);
            sb.AppendFormat("maxlength=\"{0}\" ", maxLength);
            sb.AppendFormat(" {0}>", attributes ?? string.Empty);
            if (formName != null)
            {
                sb.AppendFormat("<p class=\"formalert\" tabindex=\"0\" style=\"color: red;\" ng-show=\"{0}.{1}.$error.pattern\">{2}</p>", formName, nameAndId, errorMessage);
            }
            return new MvcHtmlString(sb.ToString());
        }

        // ls-numbers-only, but allows leading zero's
        public static MvcHtmlString NumberString(string nameAndId, string ngModel, bool required = true, string ngMin = null, string ngMax = null, int? minLength = null, int? maxLength = null, string attributes = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<input type=\"text\" ls-restrict-to-pattern ng-pattern=\"/[0-9]/\" ls-track-visited name=\"{0}\" id=\"{0}\" ng-model=\"{1}\" ", nameAndId, ngModel);
            if (required)
            {
                sb.Append("required=\"required\" ");
            }
            if (ngMin != null)
            {
                sb.AppendFormat("min=\"{0}\" ", ngMin);
            }
            if (ngMax != null)
            {
                sb.AppendFormat("max=\"{0}\" ", ngMax);
            }
            if (minLength != null)
            {
                sb.AppendFormat("ng-minlength=\"{0}\" ", minLength.Value);
            }
            if (maxLength != null)
            {
                sb.AppendFormat("maxlength=\"{0}\" ", maxLength.Value);
            }
            sb.AppendFormat(" pattern=\"\\d*\" {0}>", attributes ?? string.Empty);
            return new MvcHtmlString(sb.ToString());
        }

        // ls-numbers-only
        public static MvcHtmlString Number(string nameAndId, string ngModel, string ariaLabel, bool required = true, string ngMin = null, string ngMax = null, int? minLength = null, int? maxLength = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("<input type=\"number\" ls-numbers-only ls-track-visited name=\"{0}\" id=\"{0}\" ng-model=\"{1}\" aria-label=\"{2}\" ", nameAndId, ngModel, ariaLabel);
            if (required)
            {
                sb.Append("required=\"required\" ");
            }
            if (ngMin != null)
            {
                sb.AppendFormat("min=\"{0}\" ", ngMin);
            }
            if (ngMax != null)
            {
                sb.AppendFormat("max=\"{0}\" ", ngMax);
            }
            if (minLength != null)
            {
                sb.AppendFormat("minlength=\"{0}\" ", minLength.Value);
            }
            if (maxLength != null)
            {
                sb.AppendFormat("maxlength=\"{0}\" ", maxLength.Value);
            }
            sb.Append(" pattern=\"\\d*\" >");
            return new MvcHtmlString(sb.ToString());
        }

        // ls-currency-only
        public static MvcHtmlString Currency(string nameAndId, string ngModel, string ariaLabel,
                                             bool required = true, string ngRequired = null,
                                             string ngDisabled = null,
                                             string ngMin = null, string ngMax = null, 
                                             string ngChange = null,
                                             int? minLength = null, int? maxLength = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"<input type=\"text\" role=\"textbox\" ls-currency-only ls-track-visited name=\"{nameAndId}\" id=\"{nameAndId}\" ng-model=\"{ngModel}\" aria-label=\"{ariaLabel}\" placeholder=\"$\" ");
            if (required && ngRequired == null)
            {
                sb.Append("required=\"required\" ");
            }
            if (ngRequired != null)
            {
                sb.AppendFormat("ng-required=\"{0}\" ", ngRequired);
            }
            if (ngChange != null)
            {
                sb.AppendFormat("ng-change=\"{0}\" ", ngChange);
            }
            if (ngMin != null)
            {
                sb.AppendFormat("min=\"{0}\" ", ngMin);
            }
            if (ngMax != null)
            {
                sb.AppendFormat("max=\"{0}\" ", ngMax);
            }
            if (minLength != null)
            {
                sb.AppendFormat("minlength=\"{0}\" ", minLength.Value);
            }
            if (maxLength != null)
            {
                sb.AppendFormat("maxlength=\"{0}\" ", maxLength.Value);
            }
            if (ngDisabled != null)
            {
                sb.AppendFormat("ng-disabled=\"{0}\" ", ngDisabled);
            }

            sb.Append("  >");
            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString SocialSecurityNumber(string formName, string ngModel, string nameAndId, string ngRequired = null, bool trackVisited = false, string ngDisabled = null)
        {
            StringBuilder sb = new StringBuilder();

            // Create the <input> element string
            sb.AppendFormat("<input type=\"text\" id=\"{0}\" name=\"{0}\" ng-model=\"{1}\" maxlength=\"12\" aria-required=\"true\" ls-social-security-number pattern=\"\\d*\" aria-label=\"Enter your social security number.\"", nameAndId, ngModel);
            if (!String.IsNullOrEmpty(ngDisabled))
                sb.AppendFormat(" ng-disabled=\"{0}\"", ngDisabled);
            if (ngRequired != null)
                sb.AppendFormat(" ng-required=\"{0}\"", ngRequired);
            if(trackVisited)
                sb.Append(" ls-track-visited");
            sb.Append("/>");

            // Create <div> for the error message
            sb.Append("<div class=\"formalert\" ng-cloak=\"\"");
            if( trackVisited )
                sb.AppendFormat(" ng-show=\"{0}['{1}'].$hasVisited && !{0}['{1}'].$valid", formName, nameAndId);
            else
                sb.AppendFormat(" ng-show=\"!{0}['{1}'].$valid", formName, nameAndId);
            // Don't show errors if control is disabled
            if (!String.IsNullOrEmpty(ngDisabled))
                sb.AppendFormat(" && !({0})\">", ngDisabled);
            else
                sb.Append("\">");
            sb.Append("<p id=\"SSerrorMsg\" role=\"note\" tabindex=\"0\" aria-label=\"The social security number you entered does not match our records. Please enter a valid 9 digit social security number.\">Please enter a valid social security number (123-45-6789)</p>");
            sb.Append("</div>");

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString AccountNumber(string formName, string ngModel, string nameAndId, string ngRequired = null, bool trackVisited = false, string ngDisabled = null)
        {
            StringBuilder sb = new StringBuilder();

            // Create the <input> element string
            sb.AppendFormat("<input type=\"text\" id=\"{0}\" name=\"{0}\" ng-model=\"{1}\" ls-account-number pattern=\"\\d*\" aria-label=\"Enter your reference or account number.\"", nameAndId, ngModel);
            if (ngRequired != null)
                sb.AppendFormat(" ng-required=\"{0}\"",ngRequired);
            if (trackVisited)
                sb.Append(" ls-track-visited");
            if (!String.IsNullOrEmpty(ngDisabled))
                sb.AppendFormat(" ng-disabled=\"{0}\"", ngDisabled);
            sb.Append("/>");

            // Create <div> for the error message
            sb.Append("<div class=\"formalert\" ng-cloak=\"\"");
            if (trackVisited)
                sb.AppendFormat(" ng-show=\"{0}['{1}'].$hasVisited && !{0}['{1}'].$valid", formName, nameAndId);
            else
                sb.AppendFormat(" ng-show=\"!{0}['{1}'].$valid", formName, nameAndId);
            // Don't show errors if control is disabled
            if (!String.IsNullOrEmpty(ngDisabled))
                sb.AppendFormat(" && !({0})\">", ngDisabled);
            else
                sb.Append("\">");
            sb.Append("<p role=\"note\" tabindex=\"0\" aria-label=\"Please correct the following errors: The value entered is not a valid account number. Please provide us with either your social security number or your reference or account number to help us recover your user ID.\">Please enter a valid Account Number (e.g. 12345678)</p>");
            sb.Append("</div>");

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString AccountUserId(string formName, string ngModel, string nameAndId, string ngRequired = null, bool trackVisited = false, string ngDisabled= null)
        {
            StringBuilder sb = new StringBuilder();

            // Create the <input> element string
            sb.AppendFormat("<input type=\"text\" id=\"{0}\" name=\"{0}\" ng-model=\"{1}\" ng-pattern=\"/^[A-Za-z0-9\\!\\&\\$\\,\\-\\.\\_\\@)]+$/\" ls-restrict-to-pattern", nameAndId, ngModel);
            if (ngRequired != null)
                sb.AppendFormat(" ng-required=\"{0}\"", ngRequired);
            if (trackVisited)
                sb.Append(" ls-track-visited");
            if (!String.IsNullOrEmpty(ngDisabled))
                sb.AppendFormat(" ng-disabled=\"{0}\"", ngDisabled);
            sb.Append("/>");

            // Create <div> for the error message
            sb.Append("<div class=\"formalert\" ng-cloak=\"\"");
            if (trackVisited)
                sb.AppendFormat(" ng-show=\"{0}['{1}'].$hasVisited && !{0}['{1}'].$valid", formName, nameAndId);
            else
                sb.AppendFormat(" ng-show=\"!{0}['{1}'].$valid", formName, nameAndId);
            // Don't show errors if control is disabled
            if (!String.IsNullOrEmpty(ngDisabled))
                sb.AppendFormat(" && !({0})\">", ngDisabled);
            else
                sb.Append("\">");
            sb.Append("<p tabindex=\"0\">Please enter your User ID</p>");
            sb.Append("</div>");

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString NewPassword(string formName, string ngModel, string nameAndId, string ngRequired = null,
                                        bool restrictToPattern = true, string ngDisabled = null, bool showError = true)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<input type=\"password\" id=\"{0}\" name =\"{0}\" ng-model=\"{1}\" maxlength=\"20\" minlength=\"6\"", nameAndId, ngModel);
            if( restrictToPattern )
                sb.Append(" ng-pattern=\"/^(?=.*[0-9])(?=.*[a-zA-Z])[a-zA-Z0-9!&$,-._ ]{8,20}$/\"");
            if (!String.IsNullOrEmpty(ngRequired))
                sb.AppendFormat(" ng-required=\"{0}\"", ngRequired);
            if (!String.IsNullOrEmpty(ngDisabled))
                sb.AppendFormat(" ng-disabled=\"{0}\"", ngDisabled);
            sb.Append(" ng-focus='PasswordOnFocus = true' ls-track-visited/>");

            if (showError)
            {
                // Error message
                sb.AppendFormat("<div ng-show=\"{0}['{1}'].$hasVisited && {0}['{1}'].$error.pattern\" ng-cloak=\"\">", formName, nameAndId);
                sb.Append("<div class=\"formalert hide-no-js\">");
                sb.AppendFormat("<p tabindex=\"0\">{0}</p>", Resources.LoanAppErrorMessages.ErrorPasswordRegEx);
                sb.Append("</div>");
                sb.Append("</div>");
            }

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString BankName(string ngModel, string nameAndId, string ngRequired = null)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("<input type=\"text\" id=\"{0}\" name =\"{0}\" ng-model=\"{1}\" maxlength=\"17\" ng-maxlength=\"17\"  ng-minlength=\"2\" ", nameAndId, ngModel);
            sb.Append(" ng-pattern=\"/^[A-Za-z0-9 \\,\\-\\.\\_\\@)]+$/\" ls-restrict-to-pattern ");
            sb.AppendFormat(" ng-required=\"{0}\"", ngRequired);
            sb.Append(" />");
            return new MvcHtmlString(sb.ToString());
        }

    }
}