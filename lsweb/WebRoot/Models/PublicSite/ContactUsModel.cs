using FirstAgain.Common.Logging;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Helpers;
using LightStreamWeb.Models.Middleware;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Net.Mail;

namespace LightStreamWeb.Models.PublicSite
{
    public class ContactUsModel : BasePublicPageWithAdObjects
    {
        private ContactUsFaqPage cmsContent;

        [Required]
        [PlaceHolder("Name")]
        public string Name { get; set; }

        [Required]
        [PlaceHolder("Email")]
        [DataType(DataType.EmailAddress)]
        public string EmailAddress { get; set; }

        [Required]
        [PlaceHolder("Questions or Comments (Please search FAQs first)")]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        public bool MessageSent { get; private set; }

        public ContactUsModel() : this(new ContentManager(), AppSettings.Load().PageDefault.ContactUs)
        { }

        public ContactUsModel(ContentManager content, LightStreamPageDefault defaults)
        {
            BodyClass = defaults.BodyClass;
            NgApp = "LightStreamApp";

            cmsContent = content.Get<ContactUsFaqPage>();
            if (cmsContent != null)
            {
                SubTitle = cmsContent.SubTitle;
                PageTitle = cmsContent.PageTitle ?? defaults.PageTitle;
                if (cmsContent.MetaTagContent != null && cmsContent.MetaTagContent.MetaTagDescription != string.Empty)
                {
                    MetaDescription = cmsContent.MetaTagContent.MetaTagDescription;
                }
                if (cmsContent.MetaTagContent != null && cmsContent.MetaTagContent.MetaTagKeywords != string.Empty)
                {
                    MetaKeywords = cmsContent.MetaTagContent.MetaTagKeywords;
                }
                if (cmsContent.MetaTagContent != null && cmsContent.MetaTagContent.PageTitle != string.Empty)
                {
                    Title = cmsContent.MetaTagContent.PageTitle;
                }
                Banner = cmsContent.Banner ?? new BannerImage
                {
                    Image = new WebImage { Alt = defaults.BannerAlt },
                    HttpImageUrl = defaults.Banner
                };
                MetaImage = Banner.ToImageUrl().ToString();
            }
        }

        public void Send()
        {
            try
            {
                var toEmailInbox = BusinessConstants.Instance.CorrespondenceInboxes.Single(a => a.CorrespondenceInboxType == CorrespondenceInboxLookup.CorrespondenceInbox.CustomerService);
                using (MailMessage msg = new MailMessage())
                {
                    msg.To.Add(new MailAddress(toEmailInbox.EmailAddress, toEmailInbox.DisplayName));
                    msg.ReplyToList.Add(new MailAddress(EmailAddress, Name));
                    msg.Subject = "General Question or Comment";
                    msg.From = new MailAddress(toEmailInbox.EmailAddress, Name);
                    msg.Body = string.Format("Return Address: {0} - {1}\n\n{2}", Name, EmailAddress, Message);

                    using (SmtpClient smtpClient = new SmtpClient(ConfigurationManager.AppSettings["ExternalSMTPServer"]))
                    {
                        smtpClient.UseDefaultCredentials = true;
                        /* location of Veracode flaw 4711. Either mitigate by design, or replace with SendGrid. */
                        smtpClient.Send(msg);
                    }
                }

                MessageSent = true;
            }
            catch (Exception ex)
            {
                LightStreamLogger.WriteError(ex);
                SetGenericErrorFlag(ex);
            }
        }

        [ReadOnly(true)]
        public string SubTitle { get; protected set; }
    }
}