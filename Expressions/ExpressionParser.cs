using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Expressions;

class ExpressionParser
{
    internal static IEnumerable<Token> ParseExpressionToRpn(string expression)
    {
        var p = new TokenParser(expression);

        var output = new List<Token>();
        var operators = new Stack<Token>();

        bool lastTokenWasOperator = true;
        Token? readToken;
        while ((readToken = p.NextToken()) != null)
        {
            Token token = readToken.Value;

            string opName = token.Text;
            if (lastTokenWasOperator && opName == "-") token = new Token() { Text = "_neg", Type = TokenType.Operator };
            if (lastTokenWasOperator && opName == "+") continue;

            if (token.Type is TokenType.Number or TokenType.String)
            {
                output.Add(token);
                lastTokenWasOperator = false;
            }
            else if (token.Type == TokenType.Identifier)
            {
                if (FunctionsAndOperators.Functions.ContainsKey(token.Text.ToLower()))
                {
                    operators.Push(token);
                }
                else
                {
                    output.Add(token);
                }
                lastTokenWasOperator = false;
            }
            else if (token.Text == "(")
            {
                operators.Push(token);
                lastTokenWasOperator = true;
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
                if (operators.Any() && FunctionsAndOperators.Functions.ContainsKey(operators.Peek().Text.ToLower()))
                {
                    output.Add(operators.Pop());
                }
                lastTokenWasOperator = false;
            }
            else if (token.Type == TokenType.Operator)
            {
                var op = FunctionsAndOperators.Operators[token.Text];
                while (operators.Any() && operators.Peek().Text != "(" &&
                       (FunctionsAndOperators.Operators[operators.Peek().Text].Precidence > op.Precidence ||
                       (op.Associativity == Associativity.Left && FunctionsAndOperators.Operators[operators.Peek().Text].Precidence == op.Precidence)))
                {
                    output.Add(operators.Pop());
                }

                operators.Push(token);
                lastTokenWasOperator = true;
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