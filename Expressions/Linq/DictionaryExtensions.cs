using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace Core.Linq;

public static class DictionaryExtensions
{
    public static TValue? TryGetValue<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue? defaultValue = default)
    {
        var result = defaultValue;

        if (dictionary.TryGetValue(key, out TValue? temp))
        {
            result = temp;
        }

        return result;
    }

    public static T? TryGetAs<TKey, T>(this IDictionary<TKey, object> dictionary, TKey key)
        where T : class
    {
        T? result = null;

        if (dictionary.TryGetValue(key, out object? temp))
        {
            result = temp as T;
        }

        return result;
    }
}