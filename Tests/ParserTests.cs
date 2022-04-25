using Xunit;

#pragma warning disable IDE0051

namespace ExpressionTree.Tests;

public class ParserTests
{
    static string NextToken(TokenParser parser)
    {
        var token = parser.NextToken();
        Assert.NotNull(token);
        return token!.Value.Text;
    }

    [Fact]
    void Basics()
    {
        TokenParser parser = new("3 1 hi '321 bye don\\'t' - + -0.12 ( ) * && 0.234 foo.bar");

        Assert.Equal("3", NextToken(parser));
        Assert.Equal("1", NextToken(parser));
        Assert.Equal("hi", NextToken(parser));
        Assert.Equal("321 bye don't", NextToken(parser));
        Assert.Equal("-", NextToken(parser));
        Assert.Equal("+", NextToken(parser));
        Assert.Equal("-", NextToken(parser));
        Assert.Equal("0.12", NextToken(parser));
        Assert.Equal("(", NextToken(parser));
        Assert.Equal(")", NextToken(parser));
        Assert.Equal("*", NextToken(parser));
        Assert.Equal("&&", NextToken(parser));
        Assert.Equal("0.234", NextToken(parser));
        Assert.Equal("foo", NextToken(parser));
        Assert.Equal(".", NextToken(parser));
        Assert.Equal("bar", NextToken(parser));
        Assert.Null(parser.NextToken());
    }

    [Fact]
    void ExtraWhitespace()
    {
        TokenParser parser = new("  1     2 \t3  4  ");

        Assert.Equal("1", NextToken(parser));
        Assert.Equal("2", NextToken(parser));
        Assert.Equal("3", NextToken(parser));
        Assert.Equal("4", NextToken(parser));
        Assert.Null(parser.NextToken());
    }

    [Fact]
    void NoWhitespace()
    {
        TokenParser parser = new("1+2++4((()4&&&&2");

        Assert.Equal("1", NextToken(parser));
        Assert.Equal("+", NextToken(parser));
        Assert.Equal("2", NextToken(parser));
        Assert.Equal("+", NextToken(parser));
        Assert.Equal("+", NextToken(parser));
        Assert.Equal("4", NextToken(parser));
        Assert.Equal("(", NextToken(parser));
        Assert.Equal("(", NextToken(parser));
        Assert.Equal("(", NextToken(parser));
        Assert.Equal(")", NextToken(parser));
        Assert.Equal("4", NextToken(parser));
        Assert.Equal("&&", NextToken(parser));
        Assert.Equal("&&", NextToken(parser));
        Assert.Equal("2", NextToken(parser));
        Assert.Null(parser.NextToken());
    }

    [Fact]
    void Functions()
    {
        TokenParser parser = new("sin(x) + sin y * pow(z, w)");

        Assert.Equal("sin", NextToken(parser));
        Assert.Equal("(", NextToken(parser));
        Assert.Equal("x", NextToken(parser));
        Assert.Equal(")", NextToken(parser));
        Assert.Equal("+", NextToken(parser));
        Assert.Equal("sin", NextToken(parser));
        Assert.Equal("y", NextToken(parser));
        Assert.Equal("*", NextToken(parser));
        Assert.Equal("pow", NextToken(parser));
        Assert.Equal("(", NextToken(parser));
        Assert.Equal("z", NextToken(parser));
        Assert.Equal(",", NextToken(parser));
        Assert.Equal("w", NextToken(parser));
        Assert.Equal(")", NextToken(parser));
        Assert.Null(parser.NextToken());
    }
}
