using System;
using System.Web;
using System.Text;
using FirstAgain.Domain.Lookups.FirstLook;

namespace FirstAgain.Web.UI
{
    public static class CMSFileHyperLink
	{
		public static string GetCMSFileHyperLink(int webFileContentId)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("~/content/cmsfile?id=");
            sb.Append(webFileContentId.ToString());
			return sb.ToString();
		}
	}
}
