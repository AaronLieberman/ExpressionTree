using Expressions;
using Xunit;

namespace Tests;

public class ParserTests
{
    [Fact]
    public void Basics()
    {
        TokenParser parser = new("3 1 hi '321 bye don\\'t' - + -0.12 ( ) * 0.234 foo.bar");
        Assert.Equal("3", parser.NextToken());
        Assert.Equal("1", parser.NextToken());
        Assert.Equal("hi", parser.NextToken());
        Assert.Equal("'321 bye don\\'t'", parser.NextToken());
        Assert.Equal("-", parser.NextToken());
        Assert.Equal("+", parser.NextToken());
        Assert.Equal("-", parser.NextToken());
        Assert.Equal("0.12", parser.NextToken());
        Assert.Equal("(", parser.NextToken());
        Assert.Equal(")", parser.NextToken());
        Assert.Equal("*", parser.NextToken());
        Assert.Equal("0.234", parser.NextToken());
        Assert.Equal("foo", parser.NextToken());
        Assert.Equal(".", parser.NextToken());
        Assert.Equal("bar", parser.NextToken());
        Assert.Null(parser.NextToken());
    }
    
    [Fact]
    public void Whitespace()
    {
        TokenParser parser = new("  1     2 \t3  4  ");
        Assert.Equal("1", parser.NextToken());
        Assert.Equal("2", parser.NextToken());
        Assert.Equal("3", parser.NextToken());
        Assert.Equal("4", parser.NextToken());
        Assert.Null(parser.NextToken());
    }
}
