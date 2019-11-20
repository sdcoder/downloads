using FirstAgain.Common.Extensions;
using FirstAgain.Domain.ServiceModel.Client;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FirstAgain.Common.Web;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.GenericPartner;

namespace LightStreamWeb.Models.SignIn
{

    public class PartnerReferralSignInModel : SignInModel
    {
        private const string PARTNER_NAME_VARIABLE = "<%=DisplayName%>";
        private const string FIRST_NAME_VARIABLE = "<%=FirstName%>";
        private const string LOAN_AMOUNT_VARAIBLE = "<%=LoanAmount%>";
        private const string DEFAULT_GREETING_COPY = "LightStream has received your request.<br /><br /> You're just a few steps away from your loan offer.";
        private const string LENDING_TREE_DISPLAY_NAME = "Lending Tree";

        public string DisclosureCopy { get; set; }
        public List<string> FeatureImageCopy { get; set; }

        public List<WebImage> FeatureImages { get; set; }

        public List<WebImage> FooterImages { get; set; }

        public string GreetingCopy { get; set; }

        public bool IsLendingTree { get; set; }

        public int? PartnerId { get; set; }  // could be null if this is for LT (not Generic Partner)

        public int ApplicationId { get; set; }

        public string PartnerName
        {
            get;
            set;
        }

        public PartnerReferralSignInModel() : this(null, false, null) { }

        public PartnerReferralSignInModel(int? partnerId, bool isLendingTree, AuthenticationPage authPage, int appId = 0) : base()
        {
            IsLendingTree = isLendingTree;
            PartnerId = partnerId;
            ApplicationId = appId;
            InitializeContent(authPage);
            BodyClass = "co-brand-body";
        }

        private void InitializeContent(AuthenticationPage authPageContent = null)
        {
            if (authPageContent == null)
            {
                authPageContent = new ContentManager().Get<AuthenticationPage>();
            }

            DisclosureCopy = authPageContent.Disclosure;
            FeatureImageCopy = authPageContent.FeatureImageCopy;
            FeatureImages = authPageContent.FeatureImages;
            FooterImages = authPageContent.FooterImages;

            GenericPartnerDataSet.GenericPartnerRow genericPartner = null;
            if (PartnerId.HasValue)
            {
                genericPartner = DomainServiceUtilityOperations.GetGenericPartner(PartnerId.Value).GenericPartner.First();
            }
            else if (IsLendingTree) //For backward compatible
            {
                genericPartner = DomainServiceUtilityOperations.GetGenericPartner((int)PostingPartnerLookup.PostingPartner.LendingTree).GenericPartner.First();
            }

            //set greetings
            SetGreetings(authPageContent, genericPartner);

            //set logo
            SetLogo(genericPartner);

            //send registration email
            SendRegistrationEmail(genericPartner);

        }

        private void SetGreetings(AuthenticationPage authPageContent, GenericPartnerDataSet.GenericPartnerRow genericPartner)
        {
            if (ApplicationId != 0) 
            {
                SetGreetingVariables(authPageContent, genericPartner);
            }
            else // pipeline app or error case
            {
                GreetingCopy = DEFAULT_GREETING_COPY;
            }
        }

        private void SendRegistrationEmail(GenericPartnerDataSet.GenericPartnerRow genericPartner)
        {
            if (genericPartner != null && genericPartner.OfferRegistrationEmailEnabled && genericPartner.IsRegistrationEmailSuppressed && ApplicationId != 0)
            {
                DomainServiceWorkflowOperations.InitiateRegEmailProcess(ApplicationId);
            }
        }

        private void SetLogo(GenericPartnerDataSet.GenericPartnerRow genericPartner)
        {
            if (genericPartner != null && !genericPartner.IsLogoNull() && genericPartner.Logo.Length > 0)
            {
                PartnerLogo = genericPartner.Logo;
            }
        }

        private void SetGreetingVariables(AuthenticationPage authPageContent, GenericPartnerDataSet.GenericPartnerRow genericPartner)
        {
            var greetingCopy = authPageContent.GreetingCopy;
            //replace partner display name
            if (authPageContent.GreetingCopy.Contains(PARTNER_NAME_VARIABLE))
            {
                if (IsLendingTree) //For backward compatible
                {
                    greetingCopy = greetingCopy.Replace(PARTNER_NAME_VARIABLE, LENDING_TREE_DISPLAY_NAME);
                }
                else
                {
                    var displayName = !genericPartner.IsDisplayNameNull() ? genericPartner.DisplayName : genericPartner.Name;
                    greetingCopy = greetingCopy.Replace(PARTNER_NAME_VARIABLE, displayName);
                }
            }

            //replace applicant first name and loan amount
            if (authPageContent.GreetingCopy.Contains(FIRST_NAME_VARIABLE) || authPageContent.GreetingCopy.Contains(LOAN_AMOUNT_VARAIBLE))
            {
                var lads = DomainServiceLoanApplicationOperations.GetLoanApplicationByApplicationId(ApplicationId);
                string firstName = string.Empty;
                decimal loanAmount = 0;
                if (lads != null)
                {
                    if (lads.IsJoint)
                    {
                        firstName = lads.PrimaryApplicant.FirstName + " and " + lads.SecondaryApplicant.FirstName;
                    }
                    else
                    {
                        firstName = lads.PrimaryApplicant.FirstName;
                    }

                    loanAmount = lads.ApplicationDetail.SingleOrDefault().Amount;

                    greetingCopy = greetingCopy.Replace(FIRST_NAME_VARIABLE, firstName).Replace(LOAN_AMOUNT_VARAIBLE, loanAmount.ToString("C0"));
                }
            }

            GreetingCopy = greetingCopy;
        }
    }
}