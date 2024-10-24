﻿namespace PListSerializer.Core.Extensions;

static class DictionaryExtensions
{
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> newValueFunc)
    {
        if (dict.TryGetValue(key, out var value))
            return value;

        var newValue = newValueFunc();
        dict.Add(key, newValue);
        return newValue;
    }
}
