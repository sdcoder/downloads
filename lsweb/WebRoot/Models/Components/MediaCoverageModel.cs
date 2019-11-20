using FirstAgain.Domain.SharedTypes.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Components
{
    public class MediaCoverageModel
    {
        public string OwlCarouselId { get; set; }
        public string MediaGraphicUrl { get; set; }
        public string MobileMediaGraphicUrl { get; set; }
        public List<MediaCoverage> MediaCoverages { get; set; }

        public MediaCoverageModel()
        {
            MediaCoverages = new List<MediaCoverage>();
        }
    }
}