namespace ExampleProject.Logging
{
    /// <summary>
    /// Stores the original URL in the request before Piranha modifies it
    /// </summary>
    public class OriginalUrlStartupFilter : IStartupFilter
    {
        public const string OriginalUrlKey = "OriginalUrl";
        public const string OriginalQueryStringKey = "OriginalQueryString";

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, next) =>
            {
                context.Items[OriginalUrlKey] = GetUrl(context.Request);
                context.Items[OriginalQueryStringKey] = context.Request.QueryString.ToString();

                await next(context);
            });
            next(app);
        };

        private string GetUrl(HttpRequest request)
        {
            var host = request.Host.Host;
            if (request.Host.Port != null)
            {
                host += $":{request.Host.Port}";
            }
            return $"{request.Scheme}://{host}{request.Path}";
        }
    }
}
