using System;

// ReSharper disable once CheckNamespace
namespace Core.Linq;

public static class WeakReferenceExtensions
{
    public static T? TryGetTarget<T>(this WeakReference<T> reference)
        where T : class
    {
        return reference.TryGetTarget(out T? value) ? value : null;
    }
}