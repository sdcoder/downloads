using System.ComponentModel.DataAnnotations;
using System.Web.Security;

namespace LightStreamWeb.Models.SignIn
{
    public class PartnerSignInModel : BaseLightstreamPageModel
    {
        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z][A-Za-z\' \.]*$", ErrorMessage = "Use letters only")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression(@"^[a-zA-Z][A-Za-z\' \.]*$", ErrorMessage = "Use letters only")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(50)]
        public string EmailAddress { get; set; }

        /// <summary>
        /// validates the user name and password using the membership provider supplied.
        /// Membership provider is passed as a parameter, for mocking / unit testing
        /// </summary>
        /// <param name="membershipProvider"></param>
        /// <returns></returns>
        public LoginResult Login(MembershipProvider membershipProvider)
        {
            if (string.IsNullOrWhiteSpace(FirstName))
            {
                return LoginResult.FirstNameRequired;
            }
            if (string.IsNullOrWhiteSpace(LastName))
            {
                return LoginResult.LastNameRequired;
            }
            if (string.IsNullOrWhiteSpace(EmailAddress))
            {
                return LoginResult.EmailRequired;
            }
            //if (string.IsNullOrWhiteSpace(CompanyName))
            //{
            //    return LoginResult.CompanyNameRequired;
            //}

            return LoginResult.Success;
        }   

        public string MaintenanceMessage()
        {
            if (MaintenanceConfiguration.ScheduledMaintenanceStartTime.HasValue)
            {
                return string.Format(MaintenanceConfiguration.ScheduledMaintenanceMessage, MaintenanceConfiguration.ScheduledMaintenanceStartTime.Value.ToShortTimeString());
            }
            else
            {
                return MaintenanceConfiguration.SignInMaintenanceMessage;
            }
        }

        public static bool IsInMaintenanceMode()
        {
            if (MaintenanceConfiguration.IsInMaintenanceMode)
            {
                return true;
            }

            return false;
        }
        public enum LoginResult
        {
            FirstNameRequired,
            LastNameRequired,
            EmailRequired,
            CompanyNameRequired,
            Success
        }
    }

    public class PartnerSignInCookie
    {
        public string Token { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailAddress { get; set; }
    }
}