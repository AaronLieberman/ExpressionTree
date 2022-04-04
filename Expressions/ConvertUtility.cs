using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressions;

static class ConvertUtility
{
    public static bool? TryConvertToBool(object? o)
    {
        if (o == null) return false;
        try
        {
            return Convert.ToBoolean(o);
        }
        catch (FormatException)
        {
        }

        return false;
    }

    public static string? TryConvertToString(object? o)
    {
        if (o == null) return null;
        try
        {
            return Convert.ToString(o);
        }
        catch (FormatException)
        {
        }

        return null;
    }
}
