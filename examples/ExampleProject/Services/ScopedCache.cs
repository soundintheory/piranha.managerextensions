using System.Collections.Concurrent;

namespace ExampleProject.Services
{
    /// <summary>
    /// Simple data store that allows the caching of objects per request
    /// </summary>
    public class ScopedCache
    {
        private readonly IDictionary<string, object> _cache;

        public ScopedCache(IHttpContextAccessor context)
        {
            var httpContext = context.HttpContext!;
            if (httpContext.Items.TryGetValue("ScopedCache", out var cache) && cache is IDictionary<string, object> existing)
            {
                _cache = existing;
            }
            else
            {
                httpContext.Items["ScopedCache"] = _cache = new ConcurrentDictionary<string, object>();
            }
        }

        public TValue? Get<TValue>(string key)
        {
            return _cache.TryGetValue(key, out var output) ? (TValue)output : default;
        }

        public void Add<TValue>(string key, TValue value)
        {
            _cache.TryAdd(key, value!);
        }

        public void Remove(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }
        }

        public TValue GetOrAdd<TValue>(string key, Func<TValue> add)
        {
            if (_cache.TryGetValue(key, out var output))
            {
                return (TValue)output;
            }

            var value = add();

            if (_cache.TryAdd(key, value!))
            {
                return value;
            }

            return Get<TValue>(key)!;
        }

        public async Task<TValue> GetOrAddAsync<TValue>(string key, Func<Task<TValue>> add)
        {
            if (_cache.TryGetValue(key, out var output))
            {
                return (TValue)output;
            }

            var value = await add();

            if (_cache.TryAdd(key, value!))
            {
                return value;
            }

            return Get<TValue>(key)!;
        }
    }
}
