using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Expressions;

public class ExpressionTree
{
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

    static ExpressionTree ParseExpression(string expression)
    {
        var p = new TokenParser(expression);

        string? s;
        while ((s = p.NextToken()) != null)
        {

        }
    }
}
