using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using FirstAgain.Domain.SharedTypes.Partner;

namespace LightStreamWeb.Models.Partner
{
    public class PartnerDropViewModel : BaseLightstreamPageModel
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public string Token { get; set; }
        public List<PartnerFile> Files { get; set; } = new List<PartnerFile>();
    }

    public class PartnerFile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public DateTime UploadedAt { get; set; }
        public byte[] Thumbnail { get; set; }
    }
}