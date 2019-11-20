using FirstAgain.Domain.Lookups.FirstLook;
using LightStreamWeb.Filters;
using LightStreamWeb.Models.PublicSite;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml;
using System.Xml.Linq;
using FirstAgain.Common.Logging;
using System.IO;
using System.Text;

namespace LightStreamWeb.Controllers
{
    public class MarketingController : ApiController
    {
        [RssAuthorizeFilter]
        [HttpGet]
        [Route("api/marketing/blog")]
        public async Task<HttpResponseMessage> Get(string rssUrl, int limit = 0)
        {
            List<BlogPostModel> posts = new List<BlogPostModel>();

            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

                    System.Net.ServicePointManager.SecurityProtocol =
                        SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                    var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, rssUrl));
                    var stream = await response.Content.ReadAsStreamAsync();
                    using (XmlReader xmlReader = XmlReader.Create(new StreamReader(stream, System.Text.Encoding.UTF8)))
                    {
                        SyndicationFeed feed = SyndicationFeed.Load(xmlReader);

                        var items = feed.Items.OrderByDescending(i => i.PublishDate);

                        foreach (var item in limit == 0 ? items : items.Take(limit))
                        {
                            StringBuilder description = new StringBuilder();
                            foreach (SyndicationElementExtension extension in item.ElementExtensions)
                            {
                                XElement ele = extension.GetObject<XElement>();
                                if (ele.Name.LocalName == "encoded" && ele.Name.Namespace.ToString().Contains("content"))
                                {
                                    description.Append(ele.Value + "<br/>");
                                }
                            }
                            BlogPostModel model = new BlogPostModel
                            {
                                Title = item.Title.Text,
                                Summary = description.ToString(),
                            };

                            foreach (var link in item.Links)
                            {
                                foreach (var attr in link.AttributeExtensions.Where(i => i.Key.Name.Equals("type")))
                                {
                                    switch (attr.Value)
                                    {
                                        case "readmore":
                                            model.BlogPostUrl = link.Uri.ToString();
                                            break;
                                        case "image":
                                            model.ImageUrl = link.Uri.ToString();
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }

                            posts.Add(model);
                        }
                    }
                }

                //remove any model that contains a null property
                posts.RemoveAll(i => i.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Select(x => x.GetValue(i, null))
                    .Any(x => x == null));

                if (posts.Any())
                    return await Task.FromResult<HttpResponseMessage>(Request.CreateResponse(HttpStatusCode.OK, new
                    {
                        rssUrl,
                        posts,
                    }));

                return await Task.FromResult<HttpResponseMessage>(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            catch (Exception e)
            {
                LightStreamLogger.WriteWarning(e, "Error while loading blog post from {Url}", rssUrl);
                throw;
            }
        }
    
    }
}