using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ExpressionTree;

enum TokenType
{
    Identifier, Number, Operator, String
}

[DebuggerDisplay("{Text,nq} ({Type})")]
struct Token
{
    public TokenType Type;
    public string Text;
}

class TokenParser
{
    static readonly List<string> _operators;

    readonly string _expression;
    int _index;

    static TokenParser()
    {
        _operators = FunctionsAndOperators.Operators.Keys
            .Concat(new[] { "(", ")", })
            .ToList();
    }

    public TokenParser(string expression)
    {
        _expression = Regex.Replace(expression, "\\s+", " ");
    }

    string? ConsumeIf(Func<char, bool> matchChar)
    {
        int start = _index;
        if (_index >= _expression.Length || !matchChar(_expression[_index])) return null;
        while (_index < _expression.Length && matchChar(_expression[_index]))
        {
            _index++;
        }

        return _expression.Substring(start, _index - start);
    }

    static bool IsWordChar(char c, bool isFirstChar)
    {
        if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c == '_')) return true;
        if (!isFirstChar && (c >= '0' && c <= '9')) return true;
        return false;
    }

    static bool IsNumericChar(char c)
    {
        return (c >= '0' && c <= '9') || (c == '.');
    }

    string? ConsumeIfOperator()
    {
        var initial = _expression.Substring(_index);
        // descending length to get longer matches first
        foreach (var op in _operators.OrderByDescending(a => a.Length))
        {
            if (initial.StartsWith(op))
            {
                _index += op.Length;
                return initial.Substring(0, op.Length);
            }
        }

        return null;
    }

    string? ConsumeIfString()
    {
        if (_index >= _expression.Length || _expression[_index] is not '\'' and not '"') return null;
        char quoteChar = _expression[_index];

        int start = _index;
        var sb = new StringBuilder();
        sb.Append(quoteChar);
        _index++;

        while (_index < _expression.Length && _expression[_index] != quoteChar)
        {
            if (_expression[_index] == '\\' && _index < _expression.Length - 1) _index++;
            _index++;
        }

        if (_index < _expression.Length) _index++;

        return _expression.Substring(start, _index - start);
    }

    void SkipWhitespace()
    {
        while (_index < _expression.Length && _expression[_index] == ' ')
        {
            _index++;
        }
    }

    public Token? NextToken()
    {
        SkipWhitespace();

        int start = _index;
        string? s;
        if ((s = ConsumeIfOperator()) != null) return new Token { Type = TokenType.Operator, Text = s };
        if ((s = ConsumeIf(c => IsWordChar(c, _index == start))) != null) return new Token { Type = TokenType.Identifier, Text = s };
        if ((s = ConsumeIf(IsNumericChar)) != null) return new Token { Type = TokenType.Number, Text = s };
        if ((s = ConsumeIfString()) != null) return new Token { Type = TokenType.String, Text = s };

        return null;
    }
}
