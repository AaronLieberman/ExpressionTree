using System;
using System.Diagnostics;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Core.Linq;

public static class StringExtensions
{
    [DebuggerNonUserCode]
    [StringFormatMethod("format")]
    public static string FormatWith(this string format, params object[] args)
    {
        return string.Format(format, args);
    }

    [DebuggerNonUserCode]
    // ReSharper disable InconsistentNaming
    public static bool IEquals(this string a, string b)
        // ReSharper restore InconsistentNaming
    {
        return a.Equals(b, StringComparison.OrdinalIgnoreCase);
    }
}