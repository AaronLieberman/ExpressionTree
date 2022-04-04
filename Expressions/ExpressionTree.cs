using System;
using System.Collections.Generic;
using System.Linq;

namespace Expressions;

public class ExpressionTree
{
    readonly List<ExpressionTree> _children = new();

    public object? Value { get; private set; }
    public Func<object?[], object?>? Function { get; private set; }
    public IEnumerable<ExpressionTree> Children => _children;

    public static ExpressionTree Build(string expression)
    {
        return Build(ExpressionParser.ParseExpressionToRpn(expression));
    }

    internal static ExpressionTree Build(IEnumerable<Token> rpnExpression)
    {
        return Build(new Stack<Token>(rpnExpression));
    }

    static ExpressionTree Build(Stack<Token> rpnExpression)
    {
        var result = new ExpressionTree();

        Token last = rpnExpression.Pop();
        if (last.Type == TokenType.Number)
        {
            if (int.TryParse(last.Text, out int parsedInt))
            {
                result.Value = parsedInt;
            }
            else if (float.TryParse(last.Text, out float parsedSingle))
            {
                result.Value = parsedSingle;
            }
            else
            {
                throw new NotSupportedException($"Can't parse {last.Text} as single or int");
            }
        }
        else if (last.Type == TokenType.String)
        {
            result.Value = last.Text;
        }
        else if (last.Type == TokenType.Identifier)
        {
            if (last.Text.ToLowerInvariant() == "true")
            {
                result.Value = true;
            }
            else if (last.Text.ToLowerInvariant() == "false")
            {
                result.Value = false;
            }
            else
            {
                result.Value = last.Text;
            }
        }
        else if (FunctionsAndOperators.Operators.ContainsKey(last.Text))
        {
            FunctionInfo op = FunctionsAndOperators.Operators[last.Text];
            result.Value = last.Text;
            result.Function = op.Evaluate;
            for (int i = 0; i < op.ArgCount; i++)
            {
                result._children.Add(Build(rpnExpression));
            }
        }
        else if (FunctionsAndOperators.Functions.ContainsKey(last.Text))
        {
            result.Value = last.Text;
            result.Function = FunctionsAndOperators.Functions[last.Text].Evaluate;
            result._children.Add(Build(rpnExpression));
        }

        return result;
    }

    public object? Evaluate()
    {
        //return Function != null
        //    ? Function(Children.Select(a => a))
        //    : Value;
        throw new NotImplementedException();
    }
}
