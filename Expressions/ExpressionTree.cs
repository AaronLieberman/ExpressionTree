using System;
using System.Collections.Generic;

namespace Expressions;

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
