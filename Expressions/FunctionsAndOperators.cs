using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace Expressions;

static class FunctionsAndOperators
{
    // list of operators and precedence values from https://en.cppreference.com/w/cpp/language/operator_precedence
    // Note precedence in this chart is increasing from our normal way of thinking about it so we negate it
    public static readonly Dictionary<string, FunctionInfo> Operators =
        new (string op, int precidence, Associativity associativity, int argCount, Func<ExpressionTree[], object?> evaluate)[]
        {
            (".", 2, Associativity.Left, 2, args => throw new NotImplementedException()),
            ("_neg", 3, Associativity.Right, 1, MakeMathFunction1(a => -a, a => -a)),
            ("!", 3, Associativity.Right, 1, MakeLogicalFunction1(a => !a)),

            ("*", 5, Associativity.Left, 2, MakeMathFunction2((a, b) => a * b, (a, b) => a * b)),
            ("/", 5, Associativity.Left, 2, MakeMathFunction2((a, b) => a / b, (a, b) => a / b)),
            ("+", 6, Associativity.Left, 2, MakeMathFunction2((a, b) => a + b, (a, b) => a + b)),
            ("-", 6, Associativity.Left, 2, MakeMathFunction2((a, b) => a - b, (a, b) => a - b)),

            ("-lt", 9, Associativity.Left, 2, MakeMathFunction2((a, b) => a < b, (a, b) => a < b)),
            ("-le", 9, Associativity.Left, 2, MakeMathFunction2((a, b) => a <= b, (a, b) => a <= b)),
            ("-gt", 9, Associativity.Left, 2, MakeMathFunction2((a, b) => a > b, (a, b) => a > b)),
            ("-ge", 9, Associativity.Left, 2, MakeMathFunction2((a, b) => a >= b, (a, b) => a >= b)),
            ("-eq", 10, Associativity.Left, 2, MakeFunction2((a, b) => TestEquality(a, b))),
            ("-ne", 10, Associativity.Left, 2, MakeFunction2((a, b) => !TestEquality(a, b))),

            ("&&", 14, Associativity.Left, 2, MakeLogicalFunction2((a, b) => a && b)),
            ("||", 15, Associativity.Left, 2, MakeLogicalFunction2((a, b) => a || b)),

            (",", 17, Associativity.Left, 2, MakeFunction2((a, b) =>
                throw new InvalidOperationException("Comma operators are processed during tree building, shouldn't get this far"))),
        }.ToDictionary(a => a.op, a => new FunctionInfo(a.op, -a.precidence, a.associativity, a.argCount, args =>
        {
            if (!VerifyArgCount(args, a.argCount)) return null;
            return a.evaluate(args);
        }));

    public static readonly Dictionary<string, FunctionInfo> Functions =
        new (string op, int argCount, Func<ExpressionTree[], object?> evaluate)[]
        {
            ("sin", 1, MakeMathFunction1(null, a => (float)Math.Sin(a))),
            ("cos", 1, MakeMathFunction1(null, a => (float)Math.Cos(a))),
            ("pow", 2, MakeMathFunction2(null, (a, b) => Math.Pow(a, b))),
            ("not_null", 1, args => args[0].Evaluate() != null),
            ("if_else", 3, MakeFunction3((cond, a, b) => Convert.ToBoolean(cond.Evaluate()) ? a.Evaluate() : b.Evaluate())),
        }.ToDictionary(a => a.op, a => new FunctionInfo(a.op, 0, Associativity.Left, a.argCount, args =>
        {
            if (!VerifyArgCount(args, a.argCount)) return null;
            return a.evaluate(args);
        }));

    public static FunctionInfo GetOperatorOrFunction(string name)
    {
        return Operators.ContainsKey(name) ? Operators[name] : Functions[name];
    }

    static bool TestEquality(ExpressionTree a, ExpressionTree b)
    {
        var aValue = a.Evaluate();
        var bValue = b.Evaluate();
        if (aValue == null && bValue == null) return true;
        if ((aValue != null) != (bValue != null)) return false;

        if (aValue!.GetType() == bValue!.GetType())
        {
            return aValue.Equals(bValue);
        }

        try
        {
            var aConverter = TypeDescriptor.GetConverter(aValue.GetType());
            if (aConverter.CanConvertTo(bValue.GetType()))
            {
                var converted = aConverter.ConvertTo(bValue, aValue.GetType());
                return converted != null && converted.Equals(aValue);
            }
            var bConverter = TypeDescriptor.GetConverter(bValue.GetType());
            if (bConverter.CanConvertTo(aValue.GetType()))
            {
                var converted = bConverter.ConvertTo(aValue, bValue.GetType());
                return converted != null && converted.Equals(bValue);
            }
        }
        catch (FormatException)
        {
        }

        return false;
    }

    static Func<ExpressionTree[], object?> CatchConversions(Func<ExpressionTree[], object?> targetFunction)
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

    static Func<ExpressionTree[], object?> MakeMathFunction1(Func<int, object>? targetFunctionInt, Func<float, object> targetFunctionSingle)
    {
        return CatchConversions(MakeFunction1(a =>
        {
            var aValue = a.Evaluate();
            if (targetFunctionInt != null && aValue is int aValueInt) return targetFunctionInt(aValueInt);
            return targetFunctionSingle(Convert.ToSingle(aValue));
        }));
    }

    static Func<ExpressionTree[], object?> MakeMathFunction2(Func<int, int, object>? targetFunctionInt, Func<float, float, object> targetFunctionSingle)
    {
        return CatchConversions(MakeFunction2((a, b) =>
        {
            var aValue = a.Evaluate();
            var bValue = b.Evaluate();
            if (targetFunctionInt != null && aValue is int aValueInt && bValue is int bValueInt) return targetFunctionInt(aValueInt, bValueInt);
            return targetFunctionSingle(Convert.ToSingle(aValue), Convert.ToSingle(bValue));
        }));
    }

    static Func<ExpressionTree[], object?> MakeLogicalFunction1(Func<bool, object> targetFunction)
    {
        return CatchConversions(MakeFunction1(a => targetFunction(Convert.ToBoolean(a.Evaluate()))));
    }

    static Func<ExpressionTree[], object?> MakeLogicalFunction2(Func<bool, bool, object> targetFunction)
    {
        return CatchConversions(MakeFunction2((a, b) => targetFunction(Convert.ToBoolean(a.Evaluate()), Convert.ToBoolean(b.Evaluate()))));
    }

    static bool VerifyArgCount(ExpressionTree[] args, int expectedArgCount)
    {
        if (args.Length != expectedArgCount)
        {
            Debug.Fail($"Invoking binary function with {args.Length} args");
            return false;
        }

        return true;
    }

    static Func<ExpressionTree[], object?> MakeFunction1(Func<ExpressionTree, object?> targetFunction)
    {
        return args => targetFunction(args[0]);
    }

    static Func<ExpressionTree[], object?> MakeFunction2(Func<ExpressionTree, ExpressionTree, object?> targetFunction)
    {
        return args => targetFunction(args[0], args[1]);
    }

    static Func<ExpressionTree[], object?> MakeFunction3(Func<ExpressionTree, ExpressionTree, ExpressionTree, object?> targetFunction)
    {
        return args => targetFunction(args[0], args[1], args[2]);
    }
}
