using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

    [DebuggerHidden]
    static void ParseAndVerify(string expression, string expectedRpn)
    {
        VerifyRpn(ExpressionParser.ParseExpressionToRpn(expression), expectedRpn);
    }

    [Fact]
    void BasicMath()
    {
        ParseAndVerify("1 + 2", "1 2 +");
        ParseAndVerify("1 + 2 + 3", "1 2 + 3 +");
        ParseAndVerify("1+2+3", "1 2 + 3 +");
        ParseAndVerify("1 * 2 + 3", "1 2 * 3 +");
        ParseAndVerify("1 + 2 * 3", "1 2 3 * +");
        ParseAndVerify("(1 + 2) * 3", "1 2 + 3 *");
        ParseAndVerify("((1 + 2) * 3) + 4", "1 2 + 3 * 4 +");
        ParseAndVerify("1 + ((2 * 3) + 4)", "1 2 3 * 4 + +");
    }

    [Fact]
    void Logic()
    {
        ParseAndVerify("1 && 2", "1 2 &&");
        ParseAndVerify("1 || 2", "1 2 ||");
        ParseAndVerify("1 && 2 || 3 && 4", "1 2 && 3 4 && ||");
        ParseAndVerify("1 || 2 && 3 || 4", "1 2 3 && || 4 ||");
        ParseAndVerify("1 && 2 -eq 3 || 4", "1 2 3 -eq && 4 ||");
        ParseAndVerify("1 || 2 -eq 3 && 4", "1 2 3 -eq 4 && ||");
        ParseAndVerify("1 -eq 2 || 3 -ne 4", "1 2 -eq 3 4 -ne ||");
        ParseAndVerify("1 && 2 -eq 3 || 4", "1 2 3 -eq && 4 ||");
        ParseAndVerify("1 && 2 -eq 3 -ne 4", "1 2 3 -eq 4 -ne &&");
    }

    [Fact]
    void Text()
    {
        ParseAndVerify("1 + '321 bye don\\'t'", "1 '321 bye don\\'t' +");
        ParseAndVerify("\"bye\" - \"'hi'\" && '\"foo\"'", "\"bye\" \"'hi'\" - '\"foo\"' &&");
    }

    [Fact]
    void Unary()
    {
        ParseAndVerify("1 - 2", "1 2 -");
        ParseAndVerify("1 + -2", "1 2 _neg +");
        ParseAndVerify("-1 - -2", "1 _neg 2 _neg -");
        ParseAndVerify("(-3 * -4) - 1", "3 _neg 4 _neg * 1 -");
        ParseAndVerify("4 / -(3)", "4 3 _neg /");
        ParseAndVerify("4 / (-3)", "4 3 _neg /");
        
        ParseAndVerify("4 / (+3)", "4 3 /");
        ParseAndVerify("4 / +3", "4 3 /");

        ParseAndVerify("!true || !false", "true ! false ! ||");
    }

    [Fact]
    void SimpleFunctions()
    {
        ParseAndVerify("2 * sin(x) + cos 3", "2 x sin * 3 cos +");
        ParseAndVerify("2 + sin(x) * cos 3", "2 x sin 3 cos * +");
    }

    [Fact]
    void Lists()
    {
        ParseAndVerify("1, 2", "1 2 ,");
        ParseAndVerify("1, (2, 3), 4", "1 2 3 , , 4 ,");
    }

    [Fact]
    void MultiParamFunctions()
    {
        ParseAndVerify("pow(2, 3)", "2 3 , pow");
        ParseAndVerify("pow 2, 3", "2 pow 3 ,");
        ParseAndVerify("if_else(true, 1, 3)", "true 1 , 3 , if_else");
    }

    [Fact]
    void Identifiers()
    {
        ParseAndVerify("hello + goodbye - bye", "hello goodbye + bye -");
        ParseAndVerify("hello + goodbye - foo.bar.baz", "hello goodbye + foo bar . baz . -");
    }
}
