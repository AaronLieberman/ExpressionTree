using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Expressions;

enum Associativity { Left, Right }

struct FunctionInfo
{
    public int Precidence { get; }
    public Associativity Associativity { get; }
    public int ArgCount { get; }
    public Func<object?[], object?> Evaluate { get; }

    public FunctionInfo(int precidence, Associativity associativity, int argCount, Func<object?[], object?> evaluate)
    {
        Precidence = precidence;
        Associativity = associativity;
        ArgCount = argCount;
        Evaluate = evaluate;
    }
}

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

    public static Func<object?[], object?> MakeMathFunction1(Func<float, float> targetFunction)
    {
        return CatchConversions(MakeFunction1(a => targetFunction(Convert.ToSingle(a))));
    }

    public static Func<object?[], object?> MakeMathFunction2(Func<float, float, float> targetFunction)
    {
        return CatchConversions(MakeFunction2((a, b) => targetFunction(Convert.ToSingle(a), Convert.ToSingle(b))));
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

class ExpressionParser
{
    static readonly Dictionary<string, FunctionInfo> _operators =
        new (string op, int precidence, Associativity associativity, int argCount, Func<object?[], object?> evaluate)[]
        {
            ("+", 6, Associativity.Left, 2, FunctionMaker.MakeMathFunction2((a, b) => a + b)),
            ("-", 6, Associativity.Left, 2, FunctionMaker.MakeMathFunction2((a, b) => a - b)),
            ("*", 5, Associativity.Left, 2, FunctionMaker.MakeMathFunction2((a, b) => a * b)),
            ("/", 5, Associativity.Left, 2, FunctionMaker.MakeMathFunction2((a, b) => a / b)),
        }.ToDictionary(a => a.op, a => new FunctionInfo(a.precidence, a.associativity, a.argCount, args =>
        {
            if (a.argCount != 0 && !FunctionMaker.VerifyFunctionArgs(args, a.argCount)) return null;
            return a.evaluate(args);
        }));

    static readonly Dictionary<string, FunctionInfo> _functions =
        new (string op, int argCount, Func<object?[], object?> evaluate)[]
        {
            ("_neg", 1, FunctionMaker.MakeMathFunction1(a => -a)),
            ("sin", 1, FunctionMaker.MakeMathFunction1(a => (float)Math.Sin(a))),
            ("cos", 1, FunctionMaker.MakeMathFunction1(a => (float)Math.Cos(a))),
            ("pow", 2, FunctionMaker.MakeMathFunction2((a, b) => a * b)),
            ("not_null", 1, args => args[0] != null),
            ("if_else", 3, FunctionMaker.MakeFunction3((cond, a, b) => Convert.ToBoolean(cond) ? a  : b)),
        }.ToDictionary(a => a.op, a => new FunctionInfo(0, Associativity.Left, a.argCount, args =>
            {
                if (a.argCount != 0 && !FunctionMaker.VerifyFunctionArgs(args, a.argCount)) return null;
                return a.evaluate(args);
            }));

    public object? Value { get; private set; }
    public IEnumerable<ExpressionTree> Children { get; } = new List<ExpressionTree>();

    internal static IEnumerable<Token> ParseExpressionToRpn(string expression)
    {
        var p = new TokenParser(expression);

        var output = new List<Token>();
        var operators = new Stack<Token>();

        while (p.NextToken(out Token token))
        {
            if (token.Type is TokenType.Number or TokenType.String)
            {
                output.Add(token);
            }
            else if (token.Type == TokenType.Identifier)
            {
                if (_functions.ContainsKey(token.Text.ToLower()))
                {
                    operators.Push(token);
                }
                else
                {
                    output.Add(token);
                }
            }
            else if (token.Type == TokenType.Operator)
            {
                var op = _operators[token.Text];
                while (operators.Any() && operators.Peek().Text != "(" &&
                       _operators[operators.Peek().Text].Precidence > op.Precidence ||
                       (op.Associativity == Associativity.Left && _operators[operators.Peek().Text].Precidence == op.Precidence))
                {
                    output.Add(operators.Pop());
                }

                operators.Push(token);
            }
            else if (token.Text == "(")
            {
                operators.Push(token);
            }
            else if (token.Text == ")")
            {
                // if no operators then there are mismatched operators
                while (operators.Peek().Text != "(")
                {
                    output.Add(operators.Pop());
                }

                Debug.Assert(operators.Peek().Text == "(");
                operators.Pop();
                if (operators.Any() && _functions.ContainsKey(operators.Peek().Text.ToLower()))
                {
                    output.Add(operators.Pop());
                }
            }
        }

        while (operators.Any())
        {
            Debug.Assert(operators.Peek().Text is not "(" and not ")");
            output.Add(operators.Pop());
        }

        return output;
    }
}

public class ExpressionTree
{
    public object? Value { get; private set; }
    public IEnumerable<ExpressionTree> Children { get; } = new List<ExpressionTree>();

    public static ExpressionTree Build(string expression)
    {
        return Build(ExpressionParser.ParseExpressionToRpn(expression));
    }

    internal static ExpressionTree Build(IEnumerable<Token> rpnExpression)
    {
        throw new NotImplementedException();
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
            if (s.Contains('.'))
            {
                return float.TryParse(s, out float f) ? f : null;
            }

            return int.TryParse(s, out int i) ? i : null;
        }

        return s;
    }
}
