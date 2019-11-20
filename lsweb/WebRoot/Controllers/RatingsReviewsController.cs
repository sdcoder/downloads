using LightStreamWeb.App_State;
using LightStreamWeb.Controllers.Shared;
using LightStreamWeb.Models;
using LightStreamWeb.Models.Shared.Surveys;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Controllers
{
    public class RatingsReviewsController : BaseController
    {
        [Inject]
        public RatingsReviewsController(ICurrentUser user) : base(user) { }

        // GET: SurveyContainer
        [Route("ratingsreviews")]
        public ActionResult SurveyContainer()
        {
            return View("~/Views/RatingsReviews/BazaarVoiceSurveyContainer.cshtml", new BazaarVoiceSurveyModel());
        }

        // GET: RatingsReviews
        [Route("reviews-ratings")]
        public ActionResult RatingsReviews()
        {
            return View("~/Views/RatingsReviews/RatingsReviews.cshtml", new BasicPageModel());
        }
    }
}