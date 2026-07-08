using Microsoft.AspNetCore;
using Sentry.Extensibility;
using ExampleProject.Logging;

namespace ExampleProject.Extensions
{
    public static class SentryExtensions
    {
        public static IWebHostBuilder UseAndConfigureSentry(this IWebHostBuilder builder)
        {
            builder.ConfigureServices(s => {
                s.AddHttpContextAccessor();

                // The startup filter stores the unmodified url before Piranha gets to it
                s.AddTransient<IStartupFilter, OriginalUrlStartupFilter>();

                // The event processor ensures that the unmodified url is sent to Sentry
                s.AddScoped<ISentryEventProcessor, SentryEventProcessor>();
            });

            builder.UseSentry(o => o.SetBeforeSend(evt =>
            {
                return evt;
            }));

            return builder;
        }
    }
}
