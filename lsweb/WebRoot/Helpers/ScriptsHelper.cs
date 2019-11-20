using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;

namespace LightStreamWeb.Helpers
{
    public static class ScriptsHelper
    {
        private static readonly string _versionString;

        static ScriptsHelper()
        {
            var path = typeof(ScriptsHelper).Assembly.Location;

            _versionString = FileVersionInfo.GetVersionInfo(path).FileVersion;
        }

        public static IHtmlString RenderScriptTag(this HtmlHelper htmlHelper, string src)
        {
            var tagBuilder = new TagBuilder("script");
            tagBuilder.Attributes["type"] = "text/javascript";
            tagBuilder.Attributes["src"] = $"{VirtualPathUtility.ToAbsolute(src)}?v={_versionString}";

            return new HtmlString(tagBuilder.ToString());
        }

        // http://stackoverflow.com/questions/9655113/razor-section-inclusions-from-partial-view
        public static IHtmlString RegisteredScripts(this HtmlHelper htmlHelper)
        {
            var sb = new StringBuilder();
            var ctx = htmlHelper.ViewContext.HttpContext;
            var registeredScripts = ctx.Items["_scripts_"] as List<string>;
            if (registeredScripts != null && registeredScripts.Count > 0)
            {
                foreach (var script in registeredScripts.Distinct())
                {
                    var scriptBuilder = new TagBuilder("script");
                    scriptBuilder.Attributes["type"] = "text/javascript";
                    scriptBuilder.Attributes["src"] = script;
                    sb.AppendLine(scriptBuilder.ToString(TagRenderMode.Normal));
                }
            }

            var registeredScriptBlocks = ctx.Items["_script_blocks_"] as Stack<string>;
            if (registeredScriptBlocks != null && registeredScriptBlocks.Count > 0)
            {
                foreach (var script in registeredScriptBlocks)
                {
                    sb.AppendLine(script);
                }
            }

            var inlineScripts = ctx.Items["_inline_scripts_"] as Stack<string>;
            if (inlineScripts != null && inlineScripts.Count > 0)
            {
                sb.AppendLine("<script type=\"text/javascript\">");
                foreach (var script in inlineScripts)
                {
                    sb.AppendLine(script);
                }
                sb.AppendLine("</script>");
            }

            return new HtmlString(sb.ToString());
        }

        public static void RegisterInlineScript(this HtmlHelper htmlHelper, string script)
        {
            var ctx = htmlHelper.ViewContext.HttpContext;
            var registeredScripts = ctx.Items["_inline_scripts_"] as Stack<string>;
            if (registeredScripts == null)
            {
                registeredScripts = new Stack<string>();
                ctx.Items["_inline_scripts_"] = registeredScripts;
            }
            registeredScripts.Push(script);
        }

        public static void DeferredScriptBlock(this HtmlHelper htmlHelper, Func<dynamic, HelperResult> scriptTemplate)
        {
            var ctx = htmlHelper.ViewContext.HttpContext;
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var registeredScripts = ctx.Items["_script_blocks_"] as Stack<string>;
            if (registeredScripts == null)
            {
                registeredScripts = new Stack<string>();
                ctx.Items["_script_blocks_"] = registeredScripts;
            }
            registeredScripts.Push(scriptTemplate(null).ToString());
        }

        public static void RegisterScriptFile(this HtmlHelper htmlHelper, string script)
        {
            var ctx = htmlHelper.ViewContext.HttpContext;
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var registeredScripts = ctx.Items["_scripts_"] as List<string>;
            if (registeredScripts == null)
            {
                registeredScripts = new List<string>();
                ctx.Items["_scripts_"] = registeredScripts;
            }
            var bundle = System.Web.Optimization.BundleTable.Bundles.FirstOrDefault(b => b.Path.Contains(script));
            if (bundle != null)
            {
                var bundleScripts = System.Web.Optimization.Scripts.RenderFormat("{0}", bundle.Path).ToHtmlString().TrimEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var bundleScript in bundleScripts)
                {
                    if (!registeredScripts.Contains(bundleScript))
                    {
                        registeredScripts.Add(bundleScript);
                    }
                }
            }
            else
            {
                var src = urlHelper.Content(script);
                if (!registeredScripts.Contains(src))
                {
                    registeredScripts.Add(src + "?v=" + _versionString);
                }
            }

        }
    }
}