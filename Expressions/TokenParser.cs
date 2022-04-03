using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Expressions;

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

    static bool IsOperatorChar(char c)
    {
        return _operatorChars.Contains(c);
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

    public string? NextToken()
    {
        SkipWhitespace();
        int start = _index;

        string? s = ConsumeIf(c => IsWordChar(c, _index == start));
        if (s != null) return s;
        s = ConsumeIf(IsNumericChar);
        if (s != null) return s;
        s = ConsumeIf(IsOperatorChar);
        if (s != null) return s;
        s = ConsumeIfString();
        if (s != null) return s;

        return null;
    }
}
