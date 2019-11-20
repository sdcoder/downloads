using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Shared.Surveys
{
    public class BazaarVoiceSurveyModel
    {
        public string Environment
        {
            get
            {
                return BusinessConstants.Instance.Environment == EnvironmentLookup.Environment.Production ? "production" : "staging";
            }
        }

        public string ContainerUrl
        {
            get
            {
                // TODO: Add a business constant?
                return BusinessConstants.Instance.Environment == EnvironmentLookup.Environment.Production ?
                         "https://lightstream.com/ratingsreviews" : "https://test.lightstream.com/ratingsreviews";

            }
        }
    }
}