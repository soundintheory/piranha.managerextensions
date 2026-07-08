using ExampleProject.Auth;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace ExampleProject.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMemoryCacheSessionStore(this IServiceCollection services)
        {
            services.AddSingleton<ITicketStore, MemoryCacheTicketStore>();
            services.AddOptions<CookieAuthenticationOptions>(IdentityConstants.ApplicationScheme)
                .Configure<ITicketStore>((o, ticketStore) =>
                {
                    o.SessionStore = ticketStore;
                });

            return services;
        }
    }
}
