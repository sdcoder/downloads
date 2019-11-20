using System.ComponentModel.DataAnnotations;

namespace LightStreamWeb.Models.SignIn
{
    public class RecoverUserIDModel : BaseLightstreamPageModel
    {
        public const string BIND_FIELDS = "UserId,AccountNumber,SocialSecurityNumber,AccountNumberOrSSN";

        public RecoverUserIDModel()
        {
            BodyClass = "sign-in";
            NgApp = "LightStreamApp";
            AccountNumberOrSSN = new EnterAccountNumberOrSSNModel();
            
        }
        [MaxLength(20)]
        public string UserID { get; set; }

        public EnterAccountNumberOrSSNModel AccountNumberOrSSN { get; private set; }
    }
}
