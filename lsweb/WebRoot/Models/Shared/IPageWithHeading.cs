using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LightStreamWeb.Models.Shared
{
    public interface IPageWithHeading
    {
        string Heading { get;  }
        string BodyClass { get; }
        string TwitterURL { get; }
        string GoogleplusURL { get; }
        string YoutubeURL { get; }
        string BloggerURL { get; }
        string FacebookURL { get; }
    }
}
