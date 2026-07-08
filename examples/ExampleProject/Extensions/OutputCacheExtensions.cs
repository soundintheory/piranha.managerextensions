using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Piranha;
using Piranha.AspNetCore;

namespace ExampleProject.Extensions
{
    public static class OutputCacheExtensions
    {
        private static Type[] ExcludedFromInvalidation = new Type[]
        {
          
        };

        public static IServiceCollection AddAndConfigureOutputCache(this WebApplicationBuilder builder)
        {
            var config = builder.Configuration.GetSection("OutputCache")?.Get<OutputCacheOptions>() ?? new();

            if (config.Enabled)
            {
                builder.Services.AddTransient<IStartupFilter, OutputCacheStartupFilter>();

                return builder.Services.AddOutputCache(o =>
                {
                    o.AddBasePolicy(builder => builder
                        .AddPolicy<OutputCachePolicy>()
                        .Expire(TimeSpan.FromDays(365))
                        .Tag("all"));
                });
            }

            return builder.Services;
        }

        public static PiranhaApplicationBuilder UseOutputCacheInvalidation(this PiranhaApplicationBuilder builder)
        {
            var cache = builder.Builder.ApplicationServices.GetService<IOutputCacheStore>();

            App.Hooks.Pages.RegisterOnAfterSave(x => InvalidateCache(x, cache));
            App.Hooks.Posts.RegisterOnAfterSave(x => InvalidateCache(x, cache));
            App.Hooks.Alias.RegisterOnAfterSave(x => InvalidateCache(x, cache));
            App.Hooks.Media.RegisterOnAfterSave(x => InvalidateCache(x, cache));
            App.Hooks.GenericContent.RegisterOnAfterSave(x => InvalidateCache(x, cache));
            App.Hooks.Site.RegisterOnAfterSave(x => InvalidateCache(x, cache));
            App.Hooks.SiteContent.RegisterOnAfterSave(x => InvalidateCache(x, cache));
            App.Hooks.Param.RegisterOnAfterSave(x => InvalidateCache(x, cache));

            return builder;
        }

        private static Task InvalidateCacheAsync(object model, IOutputCacheStore? cache)
        {
            InvalidateCache(model, cache);
            return Task.CompletedTask;
        }

        private static void InvalidateCache(object model, IOutputCacheStore? cache)
        {
            if (cache != null && CanInvalidateCache(model))
            {
                cache.EvictByTagAsync("all", default).GetAwaiter().GetResult();
            }
        }

        private static bool CanInvalidateCache(object model)
        {
            return !ExcludedFromInvalidation.Any(x => x.IsInstanceOfType(model));
        }
    }

    public class OutputCacheOptions
    {
        public bool Enabled { get; set; }
    }

    public class OutputCacheStartupFilter : IStartupFilter
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
                app.UseOutputCache();
                next(app);
            };
        }
    }

    public class OutputCachePolicy : IOutputCachePolicy
    {
        public static readonly OutputCachePolicy Instance = new();

        public OutputCachePolicy()
        {
        }

        /// <inheritdoc />
        ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            var attemptOutputCaching = AttemptOutputCaching(context);
            context.EnableOutputCaching = true;
            context.AllowCacheLookup = attemptOutputCaching;
            context.AllowCacheStorage = attemptOutputCaching;
            context.AllowLocking = true;

            // Cache until midnight or at least 5 mins
            var timeUntilMidnight = DateTime.Today.AddDays(1) - DateTime.Now;
            var minTime = TimeSpan.FromMinutes(5);
            context.ResponseExpirationTimeSpan = timeUntilMidnight > minTime ? timeUntilMidnight : minTime;

            // Vary by any query by default
            context.CacheVaryByRules.QueryKeys = "*";

            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
        {
            var response = context.HttpContext.Response;

            // Verify existence of cookie headers
            if (!StringValues.IsNullOrEmpty(response.Headers.SetCookie))
            {
                context.AllowCacheStorage = false;
                return ValueTask.CompletedTask;
            }

            // Check response code
            if (response.StatusCode != StatusCodes.Status200OK)
            {
                context.AllowCacheStorage = false;
                return ValueTask.CompletedTask;
            }

            return ValueTask.CompletedTask;
        }

        private static bool AttemptOutputCaching(OutputCacheContext context)
        {
            // Check if the current request fulfills the requirements to be cached

            var request = context.HttpContext.Request;

            // Verify the method
            if (!HttpMethods.IsGet(request.Method) && !HttpMethods.IsHead(request.Method))
            {
                return false;
            }

            // Verify existence of authorization headers
            if (request.HttpContext.User?.Identity?.IsAuthenticated == true)
            {
                return false;
            }

            return
                !context.HttpContext.Request.Path.StartsWithSegments("/manager", StringComparison.OrdinalIgnoreCase) &&
                !context.HttpContext.Request.Path.StartsWithSegments("/uploads", StringComparison.OrdinalIgnoreCase) &&
                !context.HttpContext.Request.Path.StartsWithSegments("/assets", StringComparison.OrdinalIgnoreCase) &&
                !context.HttpContext.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
        }
    }
}
