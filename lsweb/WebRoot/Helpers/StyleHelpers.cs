using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace LightStreamWeb.Helpers
{
    public static class StyleHelpers
    {
        private static readonly string _versionString;

        static StyleHelpers()
        {
            var path = typeof(ScriptsHelper).Assembly.Location;

            _versionString = FileVersionInfo.GetVersionInfo(path).FileVersion;
        }

        // http://stackoverflow.com/questions/9655113/razor-section-inclusions-from-partial-view
        public static IHtmlString RegisteredStyles(this HtmlHelper htmlHelper)
        {
            var sb = new StringBuilder();
            var ctx = htmlHelper.ViewContext.HttpContext;
            var registeredStyles = ctx.Items["_styles_"] as List<string>;
            if (registeredStyles != null && registeredStyles.Count > 0)
            {
                foreach (var style in registeredStyles.Distinct())
                {
                    var linkBuilder = new TagBuilder("link");
                    linkBuilder.Attributes["rel"] = "stylesheet";
                    linkBuilder.Attributes["type"] = "text/css";
                    linkBuilder.Attributes["href"] = style;
                    sb.AppendLine(linkBuilder.ToString(TagRenderMode.Normal));
                }
            }

            var inlineStyles = ctx.Items["_inline_styles_"] as Stack<string>;
            if (inlineStyles != null && inlineStyles.Count > 0)
            {
                sb.AppendLine("<style>");
                foreach (var style in inlineStyles)
                {
                    sb.AppendLine(style);
                }
                sb.AppendLine("</style>");
            }

            return new HtmlString(sb.ToString());
        }

        public static void RegisterInlineStyle(this HtmlHelper htmlHelper, string style)
        {
            var ctx = htmlHelper.ViewContext.HttpContext;
            var registeredStyles = ctx.Items["_inline_styles_"] as Stack<string>;
            if (registeredStyles == null)
            {
                registeredStyles = new Stack<string>();
                ctx.Items["_inline_styles_"] = registeredStyles;
            }
            registeredStyles.Push(style);
        }

        public static void RegisterStyleFile(this HtmlHelper htmlHelper, string style)
        {
            var ctx = htmlHelper.ViewContext.HttpContext;
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var registeredStyles = ctx.Items["_styles_"] as List<string>;
            if (registeredStyles == null)
            {
                registeredStyles = new List<string>();
                ctx.Items["_styles_"] = registeredStyles;
            }
            var bundle = System.Web.Optimization.BundleTable.Bundles.FirstOrDefault(b => b.Path.Contains(style));
            if (bundle != null)
            {
                var bundleStyles = System.Web.Optimization.Styles.RenderFormat("{0}", bundle.Path).ToHtmlString().TrimEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var bundleStyle in bundleStyles)
                {
                    if (!registeredStyles.Contains(bundleStyle))
                    {
                        registeredStyles.Add(bundleStyle);
                    }
                }
            }
            else
            {
                var src = urlHelper.Content(style);
                if (!registeredStyles.Contains(src))
                {
                    registeredStyles.Add(src + "?v=" + _versionString);
                }
            }

        }

        public static void RegisterDeferredStyleFile(this HtmlHelper htmlHelper, string style)
        {
            var ctx = htmlHelper.ViewContext.HttpContext;
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var registeredStyles = ctx.Items["_deferred_styles_"] as List<string>;
            if (registeredStyles == null)
            {
                registeredStyles = new List<string>();
                ctx.Items["_deferred_styles_"] = registeredStyles;
            }
            var bundle = System.Web.Optimization.BundleTable.Bundles.FirstOrDefault(b => b.Path.Contains(style));
            if (bundle != null)
            {
                var bundleStyles = System.Web.Optimization.Styles.RenderFormat("{0}", bundle.Path).ToHtmlString().TrimEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var bundleStyle in bundleStyles)
                {
                    if (!registeredStyles.Contains(bundleStyle))
                    {
                        registeredStyles.Add(bundleStyle);
                    }
                }
            }
            else
            {
                var src = urlHelper.Content(style);
                if (!registeredStyles.Contains(src))
                {
                    registeredStyles.Add(src + "?v=" + _versionString);
                }
            }

        }

        public static IHtmlString DeferredStyles(this HtmlHelper htmlHelper)
        {
            var sb = new StringBuilder();
            var ctx = htmlHelper.ViewContext.HttpContext;
            var registeredStyles = ctx.Items["_deferred_styles_"] as List<string>;
            if (registeredStyles != null && registeredStyles.Count > 0)
            {
                sb.AppendLine("<noscript id=\"deferred-styles\">");
                foreach (var style in registeredStyles.Distinct())
                {
                    var linkBuilder = new TagBuilder("link");
                    linkBuilder.Attributes["rel"] = "stylesheet";
                    linkBuilder.Attributes["type"] = "text/css";
                    linkBuilder.Attributes["href"] = style;
                    sb.AppendLine(linkBuilder.ToString(TagRenderMode.Normal));
                }
                sb.AppendLine("</noscript>");

                sb.Append(@"<script>
    var loadDeferredStyles = function() {
        var addStylesNode = document.getElementById(""deferred-styles"");
        var replacement = document.createElement(""div"");
        replacement.innerHTML = addStylesNode.textContent;
        document.body.appendChild(replacement)
        addStylesNode.parentElement.removeChild(addStylesNode);
    };
    var raf = window.requestAnimationFrame || window.mozRequestAnimationFrame ||
        window.webkitRequestAnimationFrame || window.msRequestAnimationFrame;
        if (raf) raf(function() { window.setTimeout(loadDeferredStyles, 0); });
    else { 
        window.addEventListener('load', loadDeferredStyles); 
    }
</script>");
            
            }


            return new HtmlString(sb.ToString());
        }
    }


}