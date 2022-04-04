using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable IDE0051

namespace Expressions.Tests;

public class RpnTests
{
    static void VerifyRpn(IEnumerable<Token> tokens, string expected)
    {
        var actual = string.Join(" ", tokens.Select(a => a.Text));
        Assert.Equal(expected, actual);
    }

    [Fact]
    void BasicMath()
    {
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 + 2"), "1 2 +");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 + 2 + 3"), "1 2 + 3 +");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1+2+3"), "1 2 + 3 +");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 * 2 + 3"), "1 2 * 3 +");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 + 2 * 3"), "1 2 3 * +");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("(1 + 2) * 3"), "1 2 + 3 *");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("((1 + 2) * 3) + 4"), "1 2 + 3 * 4 +");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 + ((2 * 3) + 4)"), "1 2 3 * 4 + +");
    }

    [Fact]
    void Logic()
    {
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 && 2"), "1 2 &&");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 || 2"), "1 2 ||");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 && 2 || 3 && 4"), "1 2 && 3 4 && ||");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 || 2 && 3 || 4"), "1 2 3 && || 4 ||");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 && 2 -eq 3 || 4"), "1 2 3 -eq && 4 ||");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 || 2 -eq 3 && 4"), "1 2 3 -eq 4 && ||");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 -eq 2 || 3 -ne 4"), "1 2 -eq 3 4 -ne ||");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 && 2 -eq 3 || 4"), "1 2 3 -eq && 4 ||");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 && 2 -eq 3 -ne 4"), "1 2 3 -eq 4 -ne &&");
    }

    [Fact]
    void Text()
    {
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("1 + '321 bye don\\'t'"), "1 '321 bye don\\'t' +");
        VerifyRpn(ExpressionParser.ParseExpressionToRpn("\"bye\" - \"'hi'\" && '\"foo\"'"), "\"bye\" \"'hi'\" - '\"foo\"' &&");
    }

    [Fact]
    void Functions()
    {
        
    }

    [Fact]
    void Identifiers()
    {
        throw new NotImplementedException();
    }
}