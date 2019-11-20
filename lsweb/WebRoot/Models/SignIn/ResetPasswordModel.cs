using System.ComponentModel.DataAnnotations;

namespace LightStreamWeb.Models.SignIn
{
    public class ResetPasswordModel : BaseLightstreamPageModel
    {
        public const string BIND_FIELDS = "CustomerUserID,Password";
        public ResetPasswordModel()
        {
            BodyClass = "sign-in";
            NgApp = "LightStreamApp";
        }

        [MaxLength(50)]
        public string CustomerUserID {get; set;}

        [MaxLength(50)]
        public string Password { get; set; }
    }
}