using LightStreamWeb.Models.Shared;
using System.Collections.Generic;

namespace LightStreamWeb.Models.PublicSite
{
    public abstract class BasePublicPageWithSections : BasePublicPageWithAdObjects, IPageWithHeading
    {
        public BasePublicPageWithSections() : base()
        {
        }

        public BasePublicPageWithSections(ContentManager content) : base(content)
        {
        }

        public abstract IEnumerable<AccordianSection> GetSections();

        public bool SingleTabMode { get; set; }

        public string Heading { get; protected set; }
        public string SubHeading { get; protected set; }
        public string IntroParagraph { get; protected set; }

        private string _OpenTab;

        public string OpenTab
        {
            get
            {
                return _OpenTab;
            }
            set
            {
                _OpenTab = value;
                Canonical = ROOT_CANONICAL + value;
            }
        }

        public class AccordianSection
        {
            public string Title { get; set; }
            public string HREF { get; set; }
            public bool Selected { get; set; }
        }
    }
}