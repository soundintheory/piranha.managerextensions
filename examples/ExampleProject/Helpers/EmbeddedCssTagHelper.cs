using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Razor.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Encodings.Web;

namespace ExampleProject.Helpers
{
    [HtmlTargetElement("link", Attributes = "href,embedded", TagStructure = TagStructure.WithoutEndTag)]
    [HtmlTargetElement("link", Attributes = "href,embedded-force", TagStructure = TagStructure.WithoutEndTag)]
    public class EmbeddedCssTagHelper : UrlResolutionTagHelper
    {
        public EmbeddedCssTagHelper(IUrlHelperFactory urlHelperFactory, HtmlEncoder htmlEncoder, IWebHostEnvironment hostingEnvironment, TagHelperMemoryCacheProvider cacheProvider)
            :base(urlHelperFactory, htmlEncoder)
        {
            ArgumentNullException.ThrowIfNull(hostingEnvironment);
            ArgumentNullException.ThrowIfNull(cacheProvider);

            FileProvider = hostingEnvironment.WebRootFileProvider;
            Cache = cacheProvider.Cache;
        }

        public IFileProvider FileProvider { get; }

        public IMemoryCache Cache { get; }

        [HtmlAttributeName("embedded")]
        public bool Embedded { get; set; }

        [HtmlAttributeName("embedded-force")]
        public bool EmbeddedForce { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            var path = ((HtmlString)context.AllAttributes.Single(t => t.Name == "href").Value).ToString();

            if (TryResolveUrl(path, resolvedUrl: out string? resolvedUrl))
            {
                path = resolvedUrl!;
            }

            var queryStringOrFragmentStartIndex = path.AsSpan().IndexOfAny('?', '#');
            if (queryStringOrFragmentStartIndex != -1)
            {
                path = path.Substring(0, queryStringOrFragmentStartIndex);
            }

            var loadedStyles = GetLoadedStyles();

            if (EmbeddedForce || !loadedStyles.Contains(path))
            {
                var cssText = "";

                if (Cache.TryGetValue<string>(path, out var value) && value is not null)
                {
                    cssText = value;
                }
                else
                {
                    var cacheEntryOptions = new MemoryCacheEntryOptions();
                    var fileInfo = FileProvider.GetFileInfo(path);
                    var filePath = path;
                    var requestPathBase = ViewContext.HttpContext.Request.PathBase;

                    // Resolve relative path
                    if (!fileInfo.Exists &&
                        requestPathBase.HasValue &&
                        filePath.StartsWith(requestPathBase.Value, StringComparison.OrdinalIgnoreCase))
                    {
                        filePath = path.Substring(requestPathBase.Value.Length);
                        fileInfo = FileProvider.GetFileInfo(filePath);
                    }

                    cssText = GetContentForFile(fileInfo);
                    cacheEntryOptions.AddExpirationToken(FileProvider.Watch(filePath));
                    cacheEntryOptions.SetSize(cssText.Length * sizeof(char));
                    Cache.Set(path, cssText, cacheEntryOptions);
                }

                // Only render the tag if there is any CSS
                if (cssText.Length > 0)
                {
                    output.Reinitialize("style", TagMode.StartTagAndEndTag);
                    output.Content.SetHtmlContent(cssText);
                }

                loadedStyles.Add(path);
                ViewContext.HttpContext.Items["loadedStyles"] = loadedStyles;
            }
            else
            {
                output.SuppressOutput();
            }
        }

        private List<string> GetLoadedStyles()
        {
            if (ViewContext.HttpContext.Items.TryGetValue("loadedStyles", out var loadedStylesObject)
                && loadedStylesObject is List<string> loadedStyles)
            {
                return loadedStyles;
            }

            return new List<string>();
        }

        private static string GetContentForFile(IFileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return string.Empty;
            }

            var fs = fileInfo.CreateReadStream();
            using var reader = new StreamReader(fs);

            return reader.ReadToEnd();
        }
    }
}
