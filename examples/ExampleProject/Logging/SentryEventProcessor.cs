using Piranha.AspNetCore.Services;
using Sentry.Extensibility;
using System.Text;

namespace ExampleProject.Logging
{
    /// <summary>
    /// Ensures that the original url is sent with Sentry events instead of the Piranha route
    /// </summary>
    public class SentryEventProcessor(IHttpContextAccessor contextAccessor) : ISentryEventProcessor
    {
        public SentryEvent? Process(SentryEvent @event)
        {
            if (TryGetOriginalRequest(out var originalRequest))
            {
                @event.Request.Url = originalRequest.Url;
                @event.Request.QueryString = originalRequest.Query;
            }

            return @event;
        }

        private (string Url, string Query)? _originalRequest = null;

        private bool TryGetOriginalRequest(out (string Url, string Query) originalRequest)
        {
            originalRequest = (string.Empty, string.Empty);

            if (_originalRequest.HasValue)
            {
                originalRequest = _originalRequest.Value;
                return true;
            }

            if (contextAccessor?.HttpContext == null)
            {
                return false;
            }

            if (contextAccessor.HttpContext.Items[OriginalUrlStartupFilter.OriginalUrlKey] is string url 
                && contextAccessor.HttpContext.Items[OriginalUrlStartupFilter.OriginalQueryStringKey] is string qs)
            {
                _originalRequest = originalRequest = (url, qs);
                return true;
            }

            return false;
        }
    }
}
