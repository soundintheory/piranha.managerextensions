using ExampleProject.Services.Turnstile;

namespace ExampleProject.Extensions
{
    public static class TurnstileExtensions
    {
        public static IServiceCollection AddAndConfigureTurnstile(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<TurnstileOptions>(builder.Configuration.GetSection("Turnstile"));
            builder.Services.AddHttpClient<TurnstileService>();

            return builder.Services;
        }
    }
}
