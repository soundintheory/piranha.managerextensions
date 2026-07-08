namespace ExampleProject.Extensions
{
    public static class Dictionary
    {
        public static TValue? GetOrAdd<TKey,TValue>(this IDictionary<TKey,TValue> dict, TKey key, Func<TValue> value)
        {
            if (dict == null)
            {
                return default;
            }

            if (!dict.ContainsKey(key))
            {
                dict[key] = value();
            }

            return dict[key];
        }

        public static TValue? GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue>? defaultFactory = null)
        {
            if (dict != null && dict.TryGetValue(key, out var value))
            {
                return value;
            }

            return defaultFactory != null ? defaultFactory() : default;
        }

        public static T? GetOrDefault<T>(this IDictionary<string, object> dict, string key, Func<T>? defaultFactory = null)
        {
            if (dict != null && dict.TryGetValue(key, out var value) && value is T tVal)
            {
                return tVal;
            }

            return defaultFactory != null ? defaultFactory() : default;
        }

        public static T? GetOrDefault<T>(this IDictionary<string, object> dict, string key, T defaultValue)
        {
            return dict.GetOrDefault(key, () => defaultValue);
        }

        public static string? ToQueryString<TKey, TValue>(this IDictionary<TKey, TValue> dict)
        {
            if (dict == null)
            {
                return null;
            }

            var output = dict.Select((x) =>
            {
                if (x.Value == null)
                {
                    return x.Key?.ToString() ?? "-";
                }
                return $"{x.Key}={x.Value}";
            });

            return string.Join("&", output);
        }

        public static T? GetOrAdd<T>(this System.Collections.IDictionary dict, object key, Func<T> value)
        {
            if (dict == null)
            {
                return default;
            }

            if (!dict.Contains(key))
            {
                dict[key] = value();
            }

            return (T?)dict[key];
        }

        public static bool TryGetValue<T>(this System.Collections.IDictionary dict, object key, out T? value)
        {
            if (dict == null || !dict.Contains(key))
            {
                value = default;
                return false;
            }

            value = (T?)dict[key];
            return true;
        }
    }
}
