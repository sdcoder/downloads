using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LightStreamWeb.Models.SignIn
{
    public class SecurityQuestionModel : BaseLightstreamPageModel
    {
        public SecurityQuestionModel()
        {
            PageHeader = "Recover User ID";
            ChangePasswordRequired = false;
            BodyClass = "sign-in";
        }

        [ReadOnly(true)]
        [MaxLength(100)]
        public string PageHeader { get; set; }
        [MaxLength(100)]
        public string SecurityQuestion { get; set; }
        [MaxLength(100)]
        public string SecurityAnswer { get; set; }
        [ReadOnly(false)]
        public bool ChangePasswordRequired { get; set; }
    }
}