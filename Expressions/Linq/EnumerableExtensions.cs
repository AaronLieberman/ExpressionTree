using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace Core.Linq;

public static class EnumerableExtensions
{
    static readonly object _lock = new();
    static readonly Random _random = new();

    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list, Random? random = null)
    {
        var array = list.ToArray();

        for (var i = 0; i < array.Length - 1; i++)
        {
            int index;

            lock (_lock)
            {
                index = (random ?? _random).Next(i, array.Length);
            }

            (array[index], array[i]) = (array[i], array[index]);
        }

        return array;
    }

    public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
    {
        foreach (var item in list)
        {
            action(item);
        }
    }
}