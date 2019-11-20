using System.Web;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using FirstAgain.Domain.Common;
using FirstAgain.Domain.Lookups.FirstLook;
using FirstAgain.Domain.SharedTypes.ContentManagement;
using FirstAgain.Domain.ServiceModel.Client;
using Ninject;
using LightStreamWeb.App_State;
using LightStreamWeb.Models.Middleware;

namespace LightStreamWeb
{
    public class ContentManager
    {
        private readonly ICurrentHttpRequest _request;
        private readonly ICurrentUser _user;
        private readonly string _cdnBaseUrl;

        public string CdnBaseUrl { get { return _cdnBaseUrl;  } }

        public ContentManager()
        {
            _request = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<ICurrentHttpRequest>();
            _user = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<ICurrentUser>();
            _cdnBaseUrl = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<IAppSettings>().CdnBaseUrl;
        }

        public ContentManager(ICurrentHttpRequest r, ICurrentUser u)
        {
            _request = r;
            _user = u;
            _cdnBaseUrl = App_Start.NinjectWebCommon.Bootstrapper.Kernel.Get<IAppSettings>().CdnBaseUrl;
        }

        public bool IsCMSPreviewAllowed()
        {
            if (BusinessConstants.Instance.Environment != EnvironmentLookup.Environment.Production)
                return true;

            // Don't allow specific revisions to be retrieved by the public site.
            return _request.Port != 80 && _request.Port != 443;
        }

        public FileContent GetFileContent(int webFileContentId)
        {
            WebPageContentMap contentMap = DomainServiceContentManagementOperations.GetCachedPublishedWebContent();
            FileContent content = contentMap.GetFileContent(webFileContentId);

            // If this is a CMS preview request, get the uncached file if we don't have it in the cache.
            if (content == null && IsCMSPreviewAllowed())
            {
                WebContentDataSet ds = DomainServiceContentManagementOperations.GetWebFileContent(webFileContentId);
                if (ds.WebFileContent.Count == 1)
                {
                    WebContentDataSet.WebFileContentRow row = ds.WebFileContent[0];
                    content = new FileContent();
                    content.Content = row.FileContent;
                    content.FileName = row.FileName;
                }
            }
            return content;
        }

        public string GetImageUrl(FileContent image)
        {
            string rootPath = _request.RootPath;

            StringBuilder sb = new StringBuilder();
            sb.Append(rootPath);
            if (!rootPath.EndsWith("/"))
                sb.Append("/");
            sb.Append("cms/");

            AppendImagePath(sb, image);

            return sb.ToString();
        }

        public string GetAppRelativeImageUrl(FileContent image)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("~/cms/");

            AppendImagePath(sb, image);

            return sb.ToString();
        }

        private void AppendImagePath(StringBuilder sb, FileContent image)
        {
            string fileExtension = image == null ? ".png" : Path.GetExtension(image.FileName);
            /*
            if (image == null || string.IsNullOrEmpty(image.CMSPath) || HttpContext.Current.Request.Params["revision"] != null || SessionUtility.CMSWebContentId != null)
            {
                sb.Append(image == null ? 0 : image.WebFileContentId);
            }
            else
            {
                sb.Append(image.CMSPath);
            }
            */
            sb.Append(image == null ? 0 : image.WebFileContentId);

            sb.Append(fileExtension);
        }

        public LandingPageContent GetLandingPageByQueryParameterValue(string queryParameterValue) 
        {
            var list = DomainServiceContentManagementOperations.GetCachedPublishedWebContent().GetContentForType(typeof(LandingPageContent)).Cast<LandingPageContent>();
            return list.SingleOrDefault(a => a.QueryParameterValue == queryParameterValue);
        }

        public T Get<T>() where T : class, IWebPageContent, new()
        {
            T content = GetById() as T;
            if (content == null)
            {
                IEnumerable<IWebPageContent> list = DomainServiceContentManagementOperations.GetCachedPublishedWebContent().GetContentForType((new T()).GetType());

                if (list.Count() == 1)
                    return (T)list.Single();

                string type = _request.Params["type"];
                if (type != null)
                {
                    content = (T)list.SingleOrDefault(a => GetOrCreateUrlRewritePath(a) == type);
                }

                // For CMS preview, return a default instance.
                if (content == null && new LightStreamWeb.ContentManager().IsCMSPreviewAllowed())
                {
                    return (T)list.OrderByDescending(a => a.WebContentId).FirstOrDefault();
                }
            }
            return content;
        }

        public List<T> GetMultiple<T>() where T : class, IWebPageContent, new()
        {
            T preview = GetById() as T;

            List<T> list = DomainServiceContentManagementOperations.GetCachedPublishedWebContent().GetContentForType((new T()).GetType()).Select(a => (T)a).ToList();

            if (preview != null)
            {
                list = list.Where(a => a.WebContentId != preview.WebContentId).ToList();
                list.Add(preview);
            }
            return list;
        }

        public string GetOrCreateUrlRewritePath(IWebPageContent content)
        {
            if (!string.IsNullOrEmpty(content.QueryParameterValue))
                return content.QueryParameterValue;

            string value = new string(content.Name.Where(a => char.IsLetterOrDigit(a)).ToArray());
            return value;
        }

        public bool IsCmsPreview()
        {
            string id = _request.QueryString["id"];
            string rev = _request.QueryString["revision"];
            return !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(rev);
        }

        public IWebPageContent GetById()
        {
            int webContentId = -1, revision = -1;

            string id = _request.QueryString["id"];
            string rev = _request.QueryString["revision"];

            // Here, we try to make CMS preview "stick" by clinging to the revision that was originally previewed.
            // The last CMS revision to be previewed is identified in the session by id and revision number.
            // If the user navigates around the site in preview mode, we want the revision to "stick" when it's
            // revisited.  If it looks confusing, it's because it is.
            if (rev == null && _user.CMSWebContentId.HasValue && _user.CMSRevision.HasValue && (id == null || id == _user.CMSWebContentId.Value.ToString()))
            {
                webContentId = _user.CMSWebContentId.Value;
                revision = _user.CMSRevision.Value;
            }
            else
            {
                if (!int.TryParse(id, out webContentId))
                    return null;

                if (rev == null || !int.TryParse(rev, out revision))
                    revision = -1;
            }

            if (revision == -1 || !IsCMSPreviewAllowed())
            {
                WebPageContentMap contentMap = DomainServiceContentManagementOperations.GetCachedPublishedWebContent();
                return contentMap[webContentId];
            }
            else
            {
                WebContentDataSet ds = DomainServiceContentManagementOperations.GetWebContent(webContentId, revision);
                return new WebPageContentMap(ds).Content.SingleOrDefault();
            }
        }
    }
}
