using Microsoft.AspNetCore.HttpOverrides;

namespace ExampleProject.Extensions
{
    public static class ForwardedHeadersExtensions
    {
        public static IWebHostBuilder AddForwardedHeaders(this IWebHostBuilder builder)
        {
            builder.ConfigureServices(s => {
                s.AddTransient<IStartupFilter, ForwardedHeadersStartupFilter>();
            });

            return builder;
        }

        /// <summary>
        /// Startup filter for adding forwarded headers middleware to the beginning
        /// of the pipeline.
        /// </summary>
        internal class ForwardedHeadersStartupFilter : IStartupFilter
        {
            /// <summary>
            /// Configures the application builder.
            /// </summary>
            /// <param name="next">The next filter</param>
            /// <returns>The configure action</returns>
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    var options = new ForwardedHeadersOptions
                    {
                        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
                    };
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();

                    app.UseForwardedHeaders(options);
                    next(app);
                };
            }
        }
    }
}
