using FirstAgain.Domain.SharedTypes.Customer;

namespace LightStreamWeb.Models.AccountServices
{
    public class ApplicantPhonePageModel
    {
        public short AreaCode { get; set; }
        public short CentralOfficeCode { get; set; }
        public string LineNumber { get; set; }
        public string Extension { get; set; }

        public static ApplicantPhonePageModel Populate(AccountContactInfo.PhoneNumber phoneNumber)
        {
            if (phoneNumber == null)
            {
                return new ApplicantPhonePageModel();
            }

            return new ApplicantPhonePageModel
            {
                AreaCode = phoneNumber.AreaCode,
                CentralOfficeCode = phoneNumber.Exchange,
                LineNumber = phoneNumber.LocalNumber
            };
        }
    }
}