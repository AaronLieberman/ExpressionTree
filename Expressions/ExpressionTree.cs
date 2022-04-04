using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Expressions;

[DebuggerDisplay("{Value,nq}, children={Children.Length}")]
public class ExpressionTree
{
    public object? Value { get; private set; }
    public Func<ExpressionTree[], object?>? Function { get; private set; }
    public ExpressionTree[] Children { get; private set; } = Array.Empty<ExpressionTree>();

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
        else if (FunctionsAndOperators.Operators.ContainsKey(last.Text))
        {
            FunctionInfo op = FunctionsAndOperators.Operators[last.Text];
            result.Value = last.Text;
            result.Function = op.Evaluate;

            var children = new List<ExpressionTree>(op.ArgCount);
            for (int i = 0; i < op.ArgCount; i++)
            {
                var child = Build(rpnExpression);
                if (child.Value != null && child.Value.Equals(","))
                {
                    children.InsertRange(0, child.Children);
                }
                else
                {
                    children.Insert(0, child);
                }
            }

            result.Children = children.ToArray();
        }
        else if (FunctionsAndOperators.Functions.ContainsKey(last.Text))
        {
            result.Value = last.Text;
            result.Function = FunctionsAndOperators.Functions[last.Text].Evaluate;

            var child = Build(rpnExpression);
            if (child.Value != null && child.Value.Equals(","))
            {
                result.Children = child.Children;
            }
            else
            {
                result.Children = new[] { child };
            }
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

        return result;
    }

    public object? Evaluate(IPropertyResolver? propertyResolver = null)
    {
        return Function != null
            ? Function(Children.ToArray())
            : Value;
    }
}
