using System;
using System.Diagnostics;
using Xunit;

#pragma warning disable IDE0051

namespace Expressions.Tests;

public class ExpressionTests
{
    static void Check<T>(object? o, T v) where T : notnull
    {
        Assert.IsAssignableFrom<T>(o);
        if (o is T) Assert.Equal(v, o);
    }

    static object? BuildAndEval(string expression)
    {
        return ExpressionTree.Build(expression).Evaluate();
    }

    [Fact]
    void Constants()
    {
        Check(BuildAndEval("3"), 3);
        Check(BuildAndEval("3.1"), 3.1f);

        Check(BuildAndEval("\'hello\'"), "'hello'");
        Check(BuildAndEval("\"hello\""), "\"hello\"");

        Check(BuildAndEval("true"), true);
        Check(BuildAndEval("false"), false);
        Check(BuildAndEval("TrUe"), true);
        Check(BuildAndEval("FaLsE"), false);
        Check(BuildAndEval("0"), 0);
    }

    [Fact]
    void Arithmetic()
    {
        Check(BuildAndEval("3+2"), 5);
        Check(BuildAndEval("3 - 2"), 1);
        Check(BuildAndEval("2 - 3"), -1);
        Check(BuildAndEval("3 + 10 * 4 / 2 - 1"), 22);

        Check(BuildAndEval("(3 + 10) * 4 / 2 - 1"), 25);
        Check(BuildAndEval("(3 + 10) * 4 / (2 - 1)"), 52);
        Check(BuildAndEval("(5 + (2 - 3)) * 4 / 2 - 1"), 7);

        Assert.InRange(Convert.ToSingle(BuildAndEval("(3.1 + (2 - 3)) * 4 / 2 - 1")), 3.199f, 3.201f);
        Check(BuildAndEval("(3 + (2 - 3)) * 4 / 2 - 1.1"), 2.9f);
    }

    [Fact]
    void Unary()
    {
        Check(BuildAndEval("-3"), -3);
        Check(BuildAndEval("+3"), 3);
        Check(BuildAndEval("3 + -2"), 1);
        Check(BuildAndEval("-3 + 2"), -1);
        Check(BuildAndEval("-3 + -2"), -5);
    }

    [Fact]
    void SimpleLogic()
    {
        Check(BuildAndEval("!(true)"), false);
        Check(BuildAndEval("!(false)"), true);
        Check(BuildAndEval("!true"), false);
        Check(BuildAndEval("!false"), true);
        Check(BuildAndEval("!(1)"), false);
        Check(BuildAndEval("!(0)"), true);
        Check(BuildAndEval("!1"), false);
        Check(BuildAndEval("!0"), true);

        Check(BuildAndEval("!(!(false))"), false);
        Check(BuildAndEval("!(!(true))"), true);
        Check(BuildAndEval("!!false"), false);
        Check(BuildAndEval("!!true"), true);

        Check(BuildAndEval("true && true"), true);
        Check(BuildAndEval("false && true"), false);
        Check(BuildAndEval("true && false"), false);
        Check(BuildAndEval("false && false"), false);
        Check(BuildAndEval("!true && !false"), false);
        Check(BuildAndEval("true || true"), true);
        Check(BuildAndEval("false || true"), true);
        Check(BuildAndEval("true || false"), true);
        Check(BuildAndEval("false || false"), false);
        Check(BuildAndEval("!true || !false"), true);
    }

    [Fact]
    void Equality()
    {
        void F(string co, bool trueValue)
        {
            Check(BuildAndEval("1" + co + "1"), trueValue);
            Check(BuildAndEval("1" + co + "2"), !trueValue);
            Check(BuildAndEval("1" + co + "true"), trueValue);
            Check(BuildAndEval("true" + co + "1"), trueValue);
            Check(BuildAndEval("true" + co + "0"), !trueValue);
            Check(BuildAndEval("'hi'" + co + "1"), !trueValue);
            Check(BuildAndEval("'hi'" + co + "'hi'"), trueValue);
            Check(BuildAndEval("'hi'" + co + "'hI'"), !trueValue);
            Check(BuildAndEval("'hi'" + co + "'bye'"), !trueValue);
            Check(BuildAndEval("true" + co + "true"), trueValue);
        }

        F("-eq", true);
        F("-ne", false);
    }

    [Fact]
    void Conditional()
    {
        Check(BuildAndEval("if_else(true, 'hi', 'bye')"), "'hi'");
        Check(BuildAndEval("if_else(false, 'hi', 'bye')"), "'bye'");
        Check(BuildAndEval("if_else(1, 'hi', 'bye')"), "'hi'");
        Check(BuildAndEval("if_else(0, 'hi', 'bye')"), "'bye'");

        Check(BuildAndEval("if_else('hi' -eq 'hi', 1 + 3, 8)"), 4);
        Check(BuildAndEval("if_else('hi' -eq 'bye', 1 + 3, 8)"), 8);
    }

    //[Fact]
    //void Bindings()
    //{
    //    FakePropertyResolver resolver;
    //    resolver.add("context.value_3", 3);
    //    resolver.add("context.value_have", "here");
    //    resolver.add("context.value_true", true);
    //    resolver.add("context.value_false", false);

    //    checkInt(buildAndEval("context.value_3", resolver), 3);
    //    checkInt(buildAndEval("cOnTeXt.VaLuE_3", resolver), 3);
    //    check(buildAndEval("context.value_true", resolver), true);
    //    check(buildAndEval("cOnTeXt.VaLuE_true", resolver), true);
    //    check(buildAndEval("context.value_false", resolver), false);
    //    check(buildAndEval("cOnTeXt.VaLuE_false", resolver), false);
    //    check(buildAndEval("exists(context.value_3)", resolver), true);
    //    check(buildAndEval("eXiStS(cOnTeXt.VaLuE_3)", resolver), true);
    //    check(buildAndEval("exists(context.missing)", resolver), false);
    //    check(buildAndEval("eXiStS(cOnTeXt.MiSsInG)", resolver), false);
    //    checkInt(buildAndEval("exists_else(context.value_3, 4)", resolver), 3);
    //    check(buildAndEval("exists_else(context.missing, 5)", resolver), 5);

    //    check(buildAndEval("exists_else(context.value_have, 'nope')", resolver), "here");
    //    check(buildAndEval("exists_else(context.missing, 'nope')", resolver), "nope");
    //    check(buildAndEval("not(context.value_true)", resolver), false);
    //    check(buildAndEval("not(context.value_false)", resolver), true);

    //    // TODO broken right now, investigate at some point
    //    // check(buildAndEval("!(context.value_true)", resolver), false);
    //    // check(buildAndEval("!(context.value_false)", resolver), true);
    //    // check(buildAndEval("!context.value_true", resolver), false);
    //}
}
