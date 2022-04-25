using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ExpressionTree;

[DebuggerDisplay("{Value,nq}, children={Children.Length}")]
public class ExpressionTree
{
    public enum NodeType { Literal, Variable, Operator, Function }

    public NodeType Type { get; private set; }
    public object? Value { get; private set; }
    public Func<IEvalContext, ExpressionTree[], object?>? Function { get; private set; }
    public ExpressionTree[] Children { get; private set; } = Array.Empty<ExpressionTree>();

    readonly string _tokenText;

    ExpressionTree(string tokenText)
    {
        _tokenText = tokenText;
    }

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
        Token last = rpnExpression.Pop();

        var result = new ExpressionTree(last.Text);

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
        else if (FunctionsAndOperators.Operators.ContainsKey(last.Text.ToLowerInvariant()))
        {
            FunctionInfo op = FunctionsAndOperators.Operators[last.Text.ToLowerInvariant()];
            result.Type = NodeType.Operator;
            result.Value = last.Text;
            result.Function = op.Evaluate;

            var children = new List<ExpressionTree>(op.ArgCount);
            for (int i = 0; i < op.ArgCount; i++)
            {
                var child = Build(rpnExpression);
                if (child.Value != null && child.Type == NodeType.Operator && child.Value.Equals(","))
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
        else if (FunctionsAndOperators.Functions.ContainsKey(last.Text.ToLowerInvariant()))
        {
            result.Type = NodeType.Function;
            result.Value = last.Text;
            result.Function = FunctionsAndOperators.Functions[last.Text.ToLowerInvariant()].Evaluate;

            if (FunctionsAndOperators.Functions[last.Text.ToLowerInvariant()].ArgCount == 0)
            {
                result.Children = Array.Empty<ExpressionTree>();
            }
            else
            {
                var child = Build(rpnExpression);
                if (child.Value != null && child.Type == NodeType.Operator && child.Value.Equals(","))
                {
                    result.Children = child.Children;
                }
                else
                {
                    result.Children = new[] { child };
                }
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
            else if (last.Text.ToLowerInvariant() == "null")
            {
                result.Value = null;
            }
            else
            {
                result.Type = NodeType.Variable;
                result.Value = last.Text;
            }
        }

        return result;
    }

    public object? Evaluate(IEvalContext evalContext)
    {
        return Function != null
            ? Function(evalContext, Children.ToArray())
            : Value;
    }

    public override string ToString()
    {
        string result;

        List<string> list = Children.Select(a => a.ToString()).ToList();

        if (Type == NodeType.Function || Type == NodeType.Operator)
        {
            if (!Children.Any())
            {
                result = _tokenText;
            }
            else if (Children.Length == 1 || Type == NodeType.Function)
            {
                result = $"{_tokenText}({string.Join(", ", list)}";
            }
            else
            {
                string separator = _tokenText switch
                {
                    "," => ", ",
                    "." => ".",
                    _ => $" {_tokenText} "
                };

                result = string.Join(separator, list);
            }
        }
        else
        {
            result = _tokenText;
            Debug.Assert(!Children.Any());
        }

        return result;
    }
}
