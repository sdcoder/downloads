using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using LightStreamWeb.Models.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using FirstAgain.Common.Extensions;
using LightStreamWeb.Helpers;

namespace LightStreamWeb.Controllers
{
    [SessionState(SessionStateBehavior.Disabled)]
    public class ComponentsController : Controller
    {
        private const int DEFAULT_CACHE = 300;
        private const int SHORT_CACHE = 15;

        //
        // GET: /Components/HowItWorks
        [OutputCache(Duration = DEFAULT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult HowItWorks()
        {
            var cms = new LightStreamWeb.ContentManager().Get<HowItWorks>();
            if (cms != null)
            {
                return PartialView(cms);
            }
            return new EmptyResult();
        }

        public ActionResult DuringBusinessHours()
        {
            return PartialView();
        }

        //
        // GET: /Components/Footer
        [OutputCache(Duration = DEFAULT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult Footer()
        {
            var footer = new LightStreamWeb.ContentManager().Get<Footer>();
            if (footer != null)
            {
                return new ContentResult() { Content = ADAHelper.PreventMultipleBRsRepeatingBlank(footer.FooterCopy) };
            }
            return new EmptyResult();
        }

        // GET: /Components/PreDisclosuresFooter
        [OutputCache(Duration = DEFAULT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult PreDisclosuresFooter()
        {
            var footer = new LightStreamWeb.ContentManager().Get<Footer>();
            if (footer != null)
            {
                return new ContentResult() { Content = footer.PreDisclosuresFooter };
            }
            return new EmptyResult();
        }

        // GET: /Components/PreDisclosuresFooter
        [OutputCache(Duration = DEFAULT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult FundedAccountFooter()
        {
            var footer = new LightStreamWeb.ContentManager().Get<Footer>();
            if (footer != null)
            {
                return string.IsNullOrWhiteSpace(footer.FundedAccountFooter)
                    ? new ContentResult { Content = footer.FooterCopy }
                    : new ContentResult { Content = footer.FundedAccountFooter };
            }
            return new EmptyResult();
        }

        // GET: /Components/SameDayFundingDisclosure
        [OutputCache(Duration = DEFAULT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult SameDayFundingDisclosure()
        {
            return View();
        }

        // GET: /Components/Environment
        public ActionResult Environment()
        {
            return View(new EnvironmentModel());
        }

        // GET: /Components/CustomerExperienceGuarantee
        [OutputCache(Duration = DEFAULT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult CustomerExperienceGuarantee()
        {
            var cmsContent = new LightStreamWeb.ContentManager().Get<CustomerExperienceGuarantee>();
            if (cmsContent != null)
            {
                return View(cmsContent);
            }
            return new EmptyResult();
        }

        // GET: /Components/PlantATree
        [OutputCache(Duration = DEFAULT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult PlantATree()
        {
            var cmsContent = new LightStreamWeb.ContentManager().Get<PlantATree>();
            if (cmsContent != null)
            {
                return View(cmsContent);
            }
            return new EmptyResult();
        }

        public ActionResult RateTable(PurposeOfLoanLookup.PurposeOfLoan? purposeOfLoan)
        {
            ViewBag.PurposeOfLoan = purposeOfLoan.GetValueOrDefault(PurposeOfLoanLookup.PurposeOfLoan.NotSelected);
            return PartialView();
        }

        [OutputCache(Duration = SHORT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult FeaturedTestimonial(PurposeOfLoanLookup.PurposeOfLoan purpose = PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
        {
            var contentManager = new ContentManager();
            var comments = contentManager.Get<CustomerComments>();
            var allowOnHomePage = comments.Comments.Where(c => c.AllowOnHomePage).ToList();
            if (purpose != PurposeOfLoanLookup.PurposeOfLoan.NotSelected
                && allowOnHomePage.Count(c => c.PurposeOfLoan == purpose) > 0)
            {
                allowOnHomePage = allowOnHomePage.Where(c => c.PurposeOfLoan == purpose).ToList();
            }
            if (allowOnHomePage.Any())
            {
                return PartialView(allowOnHomePage.RandomElement<CustomerComment>());
            }
            return new EmptyResult();
        }

        [OutputCache(Duration = SHORT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult FeaturedTestimonialRedesign(string testimonialGraphicUrl,
                                                        string mobileTestimonialGraphicUrl,
                                                        string owlCarouselId,
                                                        PurposeOfLoanLookup.PurposeOfLoan purpose = PurposeOfLoanLookup.PurposeOfLoan.NotSelected,
                                                        string viewPath = "",
                                                        bool isMultipurposeAutoTemplate = false)
        {
            var contentManager = new ContentManager();
            var content = contentManager.Get<CustomerComments>();

            TestimonialModel model = new TestimonialModel();

            model.OwlCarouselId = owlCarouselId;
            model.TestimonialGraphicUrl = testimonialGraphicUrl;
            model.MobileTestimonialGraphicUrl = mobileTestimonialGraphicUrl;

            const string default_view_path = "~/Views/Components/FeaturedTestimonialRedesign.cshtml";

            if (string.IsNullOrWhiteSpace(viewPath)) { viewPath = default_view_path; }

            if (content == null)
                return View(model);

            if(isMultipurposeAutoTemplate)
            {
                model.Comments.AddRange(contentManager.Get<CustomerComments>()
                    .Comments
                    .Where(i => i.PurposeOfLoan  == PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchase)
                    .OrderByDescending(i => i.TimeStamp)
                    .Take(4)
                    .ToList());

                if(model.Comments.Count < 4)
                model.Comments.AddRange(contentManager.Get<CustomerComments>()
                    .Comments
                    .Where(i => i.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase)
                    .OrderByDescending(i => i.TimeStamp)
                    .Take(4)
                    .ToList());
            }

            if (model.Comments.Count < 4)
                model.Comments.AddRange(contentManager.Get<CustomerComments>()
                                         .Comments
                                         .Where(i => i.PurposeOfLoan == purpose && purpose != PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
                                         .OrderByDescending(i => i.TimeStamp)
                                         .Take(4)
                                         .ToList());

            if (model.Comments.Count < 4)
                model.Comments.AddRange(contentManager.Get<CustomerComments>()
                                         .Comments
                                         .Where(i => i.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
                                         .OrderByDescending(i => i.TimeStamp)
                                         .Take(4)
                                         .ToList());

            if (model.Comments.Count > 4)
                model.Comments.RemoveRange(4, model.Comments.Count - 4);

            return View(viewPath, model);
        }

        [OutputCache(Duration = SHORT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult MediaCoverage(PurposeOfLoanLookup.PurposeOfLoan purpose = PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
        {
            var contentManager = new ContentManager();
            var mediaCoverages = contentManager.Get<MediaCoverages>();
            var allowOnHomePage = mediaCoverages.MediaCoveragesCollection.Where(c => c.AllowOnHomePage).ToList();
            if (purpose != PurposeOfLoanLookup.PurposeOfLoan.NotSelected
                && allowOnHomePage.Count(c => c.PurposeOfLoan == purpose) > 0)
            {
                allowOnHomePage = allowOnHomePage.Where(c => c.PurposeOfLoan == purpose).ToList();
            }
            if (allowOnHomePage.Any())
            {
                return PartialView(allowOnHomePage.RandomElement<MediaCoverage>());
            }
            return new EmptyResult();
        }

        [OutputCache(Duration = SHORT_CACHE, VaryByCustom = "IsCMSPreview")]
        public ActionResult MediaCoverageRedesign(string mediaGraphicUrl, 
            string mobileMediaGraphicUrl, 
            string owlCarouselId, PurposeOfLoanLookup.PurposeOfLoan purpose = PurposeOfLoanLookup.PurposeOfLoan.NotSelected,
            bool isMultipurposeAutoTemplate = false)
        {
            var contentManager = new ContentManager();
            var content = contentManager.Get<MediaCoverages>();

            MediaCoverageModel model = new MediaCoverageModel();

            model.OwlCarouselId = owlCarouselId;
            model.MediaGraphicUrl = mediaGraphicUrl;
            model.MobileMediaGraphicUrl = mobileMediaGraphicUrl;

            if (content == null)
                return View(model);


            if (isMultipurposeAutoTemplate)
            {
                model.MediaCoverages.AddRange(content.MediaCoveragesCollection
                    .Where(i => i.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.UsedAutoPurchase)
                    .OrderByDescending(i => i.TimeStamp)
                    .Take(4)
                    .ToList());

                if (model.MediaCoverages.Count < 4)
                    model.MediaCoverages.AddRange(content.MediaCoveragesCollection
                        .Where(i => i.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.NewAutoPurchase)
                        .OrderByDescending(i => i.TimeStamp)
                        .Take(4)
                        .ToList());
            }

            if (model.MediaCoverages.Count < 4)
                model.MediaCoverages.AddRange(content.MediaCoveragesCollection
                                           .Where(i => i.PurposeOfLoan == purpose && purpose != PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
                                           .OrderByDescending(i => i.TimeStamp)
                                           .Take(4)
                                           .ToList());

            if (model.MediaCoverages.Count < 4)
                model.MediaCoverages.AddRange(content.MediaCoveragesCollection
                                           .Where(i => i.PurposeOfLoan == PurposeOfLoanLookup.PurposeOfLoan.NotSelected)
                                           .OrderByDescending(i => i.TimeStamp)
                                           .Take(4)
                                           .ToList());

            if (model.MediaCoverages.Count > 4)
                model.MediaCoverages.RemoveRange(4, model.MediaCoverages.Count - 4);

            return View(model);
        }
    }
}
