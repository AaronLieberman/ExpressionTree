using System;
using System.Diagnostics;
using System.Linq;

namespace Expressions;

static class FunctionMaker
{
    public static Func<object?[], object?> CatchConversions(Func<object?[], object?> targetFunction)
    {
        return args =>
        {
            try
            {
                return targetFunction(args);
            }
            catch (Exception ex) when (ex is FormatException or OverflowException)
            {
                return null;
            }
        };
    }

    public static Func<object?[], object?> MakeMathFunction1(Func<float, object> targetFunction)
    {
        return CatchConversions(MakeFunction1(a => targetFunction(Convert.ToSingle(a))));
    }

    public static Func<object?[], object?> MakeMathFunction2(Func<float, float, object> targetFunction)
    {
        return CatchConversions(MakeFunction2((a, b) => targetFunction(Convert.ToSingle(a), Convert.ToSingle(b))));
    }

    public static Func<object?[], object?> MakeLogicalFunction1(Func<bool, object> targetFunction)
    {
        return CatchConversions(MakeFunction1(a => targetFunction(Convert.ToBoolean(a))));
    }

    public static Func<object?[], object?> MakeLogicalFunction2(Func<bool, bool, object> targetFunction)
    {
        return CatchConversions(MakeFunction2((a, b) => targetFunction(Convert.ToBoolean(a), Convert.ToBoolean(b))));
    }

    public static bool VerifyFunctionArgs(object?[] args, int expectedArgCount, bool allowNullParams = false)
    {
        if (args.Length != expectedArgCount)
        {
            Debug.Fail($"Invoking binary function with {args.Length} args");
            return false;
        }

        if (!allowNullParams || args.All(a => a != null))
        {
            Debug.Fail("Parameter to binary function is null");
            return false;
        }

        return true;
    }

    public static Func<object?[], object?> MakeFunction1(Func<object, object?> targetFunction)
    {
        return args => targetFunction(args[0]!);
    }

    public static Func<object?[], object?> MakeFunction2(Func<object, object, object?> targetFunction)
    {
        return args => targetFunction(args[0]!, args[1]!);
    }

    public static Func<object?[], object?> MakeFunction3(Func<object, object, object, object?> targetFunction)
    {
        return args => targetFunction(args[0]!, args[1]!, args[2]!);
    }
}