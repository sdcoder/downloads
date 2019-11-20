using FirstAgain.Domain.SharedTypes.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.Components
{
    public class TestimonialModel
    {
        public string OwlCarouselId { get; set; }
        public string TestimonialGraphicUrl { get; set; }
        public string MobileTestimonialGraphicUrl { get; set; }
        public List<CustomerComment> Comments { get; set; }

        public TestimonialModel()
        {
            Comments = new List<CustomerComment>();
        }
    }
}