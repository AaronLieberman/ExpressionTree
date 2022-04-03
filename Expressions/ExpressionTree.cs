using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Expressions;

struct FunctionInfo
{
    public int Precidence { get; }
    public int ArgCount { get; }
    public Func<object?[], object?> Evaluate { get; }

    public FunctionInfo(int precidence, int argCount, Func<object?[], object?> evaluate)
    {
        Precidence = precidence;
        ArgCount = argCount;
        Evaluate = evaluate;
    }

    public static Func<object?[], object?> MakeBinaryMathFunction(Func<float, float, float> targetFunction)
    {
        return MakeBinaryFunction(
            (a, b) =>
            {
                try
                {
                    return targetFunction(Convert.ToSingle(a), Convert.ToSingle(b));
                }
                catch (FormatException)
                {
                    return null;
                }
                catch (OverflowException)
                {
                    return null;
                }
            }
        );
    }

    public static Func<object?[], object?> MakeBinaryFunction(Func<object, object, object?> targetFunction)
    {
        return args =>
        {
            if (args.Length != 2)
            {
                Debug.Fail($"Invoking binary function with {args.Length} args");
                return null;
            }

            if (args[0] is null || args[1] is null)
            {
                Debug.Fail("Parameter to binary function is null");
                return null;
            }

            return targetFunction(args[0]!, args[1]!);
        };
    }
}

public class ExpressionTree
{
    readonly Dictionary<string, FunctionInfo> _operators = new()
    {
        { "+", new FunctionInfo(6, 2, FunctionInfo.MakeBinaryMathFunction((a, b) => a + b)) },
        { "-", new FunctionInfo(6, 2, FunctionInfo.MakeBinaryMathFunction((a, b) => a - b)) },
        { "*", new FunctionInfo(5, 2, FunctionInfo.MakeBinaryMathFunction((a, b) => a * b)) },
        { "/", new FunctionInfo(5, 2, FunctionInfo.MakeBinaryMathFunction((a, b) => a / b)) },
    };

    public object? Value { get; private set; }
    public IEnumerable<ExpressionTree> Children { get; } = new List<ExpressionTree>();

    public static ExpressionTree Build(string expression)
    {
        return ParseExpression(expression);
    }

    public object? Evaluate()
    {
        return Value is Func<object, ExpressionTree[]> func
            ? func(Children)
            : Value;
    }

    static object? ParseAsConstant(string s)
    {
        if (s.Length == 0) return null;
        if (s.Equals("true", StringComparison.InvariantCultureIgnoreCase)) return true;
        if (s.Equals("false", StringComparison.InvariantCultureIgnoreCase)) return false;

        if (s[0] is >= '0' and <= '9')
        {
            if (s.Contains("."))
            {
                return float.TryParse(s, out float v) ? v : null;
            }

            return int.TryParse(s, out int v) ? v : null;
        }

        return s;
    }

    static object? ParseAsOperator(string s)
    {

    }

    static ExpressionTree ParseExpression(string expression)
    {
        var p = new TokenParser(expression);
        ExpressionTree result = new ExpressionTree();

        string? s;
        while ((s = p.NextToken()) != null)
        {

            result.Value = s;
        }

        return result;
    }
}
