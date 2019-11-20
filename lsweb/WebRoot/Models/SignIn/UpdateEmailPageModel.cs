using System.ComponentModel.DataAnnotations;

namespace LightStreamWeb.Models.SignIn
{
    public class UpdateEmailPageModel : BaseLightstreamPageModel
    {
        public const string BIND_FIELDS = "SecurityAnswer,OldEmailAddress,NewEmailAddress";

        public UpdateEmailPageModel()
        {
            BodyClass = "sign-in";
        }

        [MaxLength(255)]
        public string SecurityAnswer { get; set; }

        [MaxLength(255)]
        [DataType(DataType.EmailAddress)]
        public string OldEmailAddress { get; set; }

        [MaxLength(255)]
        [DataType(DataType.EmailAddress)]
        public string NewEmailAddress { get; set; }
    }
}