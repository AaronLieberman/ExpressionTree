using System;

// ReSharper disable once CheckNamespace
namespace Core.Linq;

public static class DisposableOperations
{
    public static void DisposeAndNullOut(ref IDisposable? disposable)
    {
        disposable?.Dispose();
        disposable = null;
    }
}