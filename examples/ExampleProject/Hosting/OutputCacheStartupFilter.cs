using Microsoft.AspNetCore.HttpOverrides;

namespace ExampleProject.Web.Hosting
{
    internal class OutputCacheStartupFilter : IStartupFilter
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
}
