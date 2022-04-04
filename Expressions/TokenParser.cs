using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Expressions;

enum TokenType
{
    Identifier, Number, Operator, String
}

struct Token
{
    public TokenType Type;
    public string Text;
}

class TokenParser
{
    static readonly char[] _operatorChars;

    readonly string _expression;
    int _index;

    static TokenParser()
    {
        _operatorChars = "+-*/ &| () ,".Replace(" ", "").ToCharArray();
    }

    public TokenParser(string expression)
    {
        _expression = Regex.Replace(expression, "\\s+", " ");
    }

    bool ConsumeIf(Func<char, bool> matchChar, out string s)
    {
        s = "";
        int start = _index;
        if (_index >= _expression.Length || !matchChar(_expression[_index])) return false;
        while (_index < _expression.Length && matchChar(_expression[_index]))
        {
            _index++;
        }

        s = _expression.Substring(start, _index - start);
        return true;
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

    static bool IsOperatorChar(char c)
    {
        return _operatorChars.Contains(c);
    }

    bool ConsumeIfString(out string s)
    {
        s = "";
        if (_index >= _expression.Length || _expression[_index] is not '\'' and not '"') return false;
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

        s = _expression.Substring(start, _index - start);
        return true;
    }

    void SkipWhitespace()
    {
        while (_index < _expression.Length && _expression[_index] == ' ')
        {
            _index++;
        }
    }

    public bool NextToken(out Token token)
    {
        SkipWhitespace();
        int start = _index;

        if (ConsumeIf(c => IsWordChar(c, _index == start), out string s))
        {
            token = new Token { Type = TokenType.Identifier, Text = s };
            return true;
        }

        if (ConsumeIf(IsNumericChar, out s))
        {
            token = new Token {Type = TokenType.Number, Text = s};
            return true;
        }

        if (ConsumeIf(IsOperatorChar, out s))
        {
            token = new Token { Type = TokenType.Operator, Text = s };
            return true;
        }

        if (ConsumeIfString(out s))
        {
            token = new Token { Type = TokenType.String, Text = s };
            return true;
        }

        token = new Token();
        return false;
    }
}
