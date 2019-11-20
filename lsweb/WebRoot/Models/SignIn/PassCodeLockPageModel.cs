namespace LightStreamWeb.Models.SignIn
{
    public class PassCodeLockPageModel : BaseLightstreamPageModel
    {
        public const string BIND_FIELDS = "PassCode";

        public PassCodeLockPageModel()
        {
            BodyClass = "sign-in";
        }

        public string PassCode { get; set; }
    }
}