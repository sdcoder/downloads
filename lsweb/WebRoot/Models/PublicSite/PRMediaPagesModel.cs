using FirstAgain.Domain.SharedTypes.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LightStreamWeb.Models.PublicSite
{
    public class PRMediaPagesModel : BasePublicPageWithSections
    {
        public PRMediaPagesModel()
        {
            BodyClass = "about sub";
        }

        public override IEnumerable<AccordianSection> GetSections()
        {
            var sections = new List<AccordianSection>() {
                new AccordianSection() {
                    Title = ContentManager.Get<PressKit>().Name, 
                    HREF = "press-kit"
                },
                new AccordianSection() {
                    Title = ContentManager.Get<PressReleasePage>().Name, 
                    HREF = "press-releases"
                },
                new AccordianSection() {
                    Title = "In The News", 
                    HREF = "in-the-news"
                }
            };

            if (sections.Any(s => s.HREF == OpenTab))
            {
                sections.First(s => s.HREF == OpenTab).Selected = true;

                if (SingleTabMode)
                {
                    return sections.Where(s => s.HREF == OpenTab);
                }
            }

            return sections;
        }

    }
}