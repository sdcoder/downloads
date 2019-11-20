using System.ComponentModel.DataAnnotations;

namespace LightStreamWeb.Models.SignIn
{
    public class AccountLockPageModel : BaseLightstreamPageModel
    {
        public AccountLockPageModel()
        {
            BodyClass = "sign-in";
        }

        [MaxLength(255)]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }
    }
}