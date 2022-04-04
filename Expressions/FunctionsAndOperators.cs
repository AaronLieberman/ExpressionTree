using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Linq;

namespace Expressions;

static class FunctionsAndOperators
{
    // list of operators and precedence values from https://en.cppreference.com/w/cpp/language/operator_precedence
    // Note precedence in this chart is increasing from our normal way of thinking about it so we negate it
    public static readonly Dictionary<string, FunctionInfo> Operators =
        new (string op, int precidence, Associativity associativity, int argCount, Func<object?[], object?> evaluate)[]
        {
            (".", 2, Associativity.Left, 2, args => throw new NotImplementedException()),
            ("_neg", 3, Associativity.Left, 1, MakeMathFunction1(a => -a)),
            ("!", 3, Associativity.Left, 1, MakeLogicalFunction1(a => !a)),

            ("*", 5, Associativity.Left, 2, MakeMathFunction2((a, b) => a * b)),
            ("/", 5, Associativity.Left, 2, MakeMathFunction2((a, b) => a / b)),
            ("+", 6, Associativity.Left, 2, MakeMathFunction2((a, b) => a + b)),
            ("-", 6, Associativity.Left, 2, MakeMathFunction2((a, b) => a - b)),

            ("-lt", 9, Associativity.Left, 2, MakeMathFunction2((a, b) => a < b)),
            ("-le", 9, Associativity.Left, 2, MakeMathFunction2((a, b) => a <= b)),
            ("-gt", 9, Associativity.Left, 2, MakeMathFunction2((a, b) => a > b)),
            ("-ge", 9, Associativity.Left, 2, MakeMathFunction2((a, b) => a >= b)),
            ("-eq", 10, Associativity.Left, 2, MakeLogicalFunction2((a, b) => a == b)),
            ("-ne", 11, Associativity.Left, 2, MakeLogicalFunction2((a, b) => a != b)),

            ("&&", 14, Associativity.Left, 2, MakeLogicalFunction2((a, b) => a && b)),
            ("||", 15, Associativity.Left, 2, MakeLogicalFunction2((a, b) => a || b)),

            (",", 17, Associativity.Left, 2, MakeFunction2((a, b) =>
            {
                var result = new List<object>();
                result.AddRange(a is IEnumerable<object> aEnumerable ? aEnumerable : a.WrapEnumerable());
                result.AddRange(b is IEnumerable<object> bEnumerable ? bEnumerable : b.WrapEnumerable());
                return result;
            })),
        }.ToDictionary(a => a.op, a => new FunctionInfo(a.op, -a.precidence, a.associativity, a.argCount, args =>
        {
            if (a.argCount != 0 && !VerifyFunctionArgs(args, a.argCount)) return null;
            return a.evaluate(args);
        }));

    public static readonly Dictionary<string, FunctionInfo> Functions =
        new (string op, int argCount, Func<object?[], object?> evaluate)[]
        {
            ("sin", 1, MakeMathFunction1(a => (float)Math.Sin(a))),
            ("cos", 1, MakeMathFunction1(a => (float)Math.Cos(a))),
            ("pow", 2, MakeMathFunction2((a, b) => Math.Pow(a, b))),
            ("not_null", 1, args => args[0] != null),
            ("if_else", 3, MakeFunction3((cond, a, b) => Convert.ToBoolean(cond) ? a  : b)),
        }.ToDictionary(a => a.op, a => new FunctionInfo(a.op, 0, Associativity.Left, a.argCount, args =>
        {
            if (a.argCount != 0 && !VerifyFunctionArgs(args, a.argCount)) return null;
            return a.evaluate(args);
        }));

    static Func<object?[], object?> CatchConversions(Func<object?[], object?> targetFunction)
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

    static Func<object?[], object?> MakeMathFunction1(Func<float, object> targetFunction)
    {
        return CatchConversions(MakeFunction1(a => targetFunction(Convert.ToSingle(a))));
    }

    static Func<object?[], object?> MakeMathFunction2(Func<float, float, object> targetFunction)
    {
        return CatchConversions(MakeFunction2((a, b) => targetFunction(Convert.ToSingle(a), Convert.ToSingle(b))));
    }

    static Func<object?[], object?> MakeLogicalFunction1(Func<bool, object> targetFunction)
    {
        return CatchConversions(MakeFunction1(a => targetFunction(Convert.ToBoolean(a))));
    }

    static Func<object?[], object?> MakeLogicalFunction2(Func<bool, bool, object> targetFunction)
    {
        return CatchConversions(MakeFunction2((a, b) => targetFunction(Convert.ToBoolean(a), Convert.ToBoolean(b))));
    }

    static bool VerifyFunctionArgs(object?[] args, int expectedArgCount, bool allowNullParams = false)
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

    static Func<object?[], object?> MakeFunction1(Func<object, object?> targetFunction)
    {
        return args => targetFunction(args[0]!);
    }

    static Func<object?[], object?> MakeFunction2(Func<object, object, object?> targetFunction)
    {
        return args => targetFunction(args[0]!, args[1]!);
    }

    static Func<object?[], object?> MakeFunction3(Func<object, object, object, object?> targetFunction)
    {
        return args => targetFunction(args[0]!, args[1]!, args[2]!);
    }
}
