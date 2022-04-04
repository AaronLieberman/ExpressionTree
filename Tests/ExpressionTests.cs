using System;
using System.Diagnostics;
using Xunit;

#pragma warning disable IDE0051

namespace Expressions.Tests;

public class ExpressionTests
{
    #region Check

    [DebuggerHidden]
    void Check<T>(object? o, T v)
    {
        Debug.Assert(o is T);
        if (o is T)
        {
            Debug.Assert(o.Equals(v));
        }
    }

    #endregion

    static object? BuildAndEval(string expression)
    {
        return ExpressionTree.Build(expression).Evaluate();
    }

    [Fact]
    void Constants()
    {
        Check(BuildAndEval("3"), 3.0);

        Check(BuildAndEval("\'hello\'"), "hello");
        Check(BuildAndEval("\"hello\""), "hello");

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
        Check(BuildAndEval("3 + 10 * 4 / 2 - 1"), 22);

        Check(BuildAndEval("(3 + 10) * 4 / 2 - 1"), 25);
        Check(BuildAndEval("(3 + 10) * 4 / (2 - 1)"), 52);
        Check(BuildAndEval("(3 + (2 - 3)) * 4 / 2 - 1"), 7);
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
    void SimpleFunctions()
    {
        Check(BuildAndEval("not(true)"), false);
        Check(BuildAndEval("NoT(true)"), false);
        Check(BuildAndEval("not(false)"), true);
        Check(BuildAndEval("NoT(false)"), true);
        Check(BuildAndEval("not(1)"), false);
        Check(BuildAndEval("not(0)"), true);

        Check(BuildAndEval("not(not(false))"), false);
        Check(BuildAndEval("not(not(true))"), true);

        Check(BuildAndEval("_and(true, true)"), true);
        Check(BuildAndEval("_and(false, true)"), false);
        Check(BuildAndEval("_and(true, false)"), false);
        Check(BuildAndEval("_and(false, false)"), false);
        Check(BuildAndEval("_and(not(true), not(false))"), false);
        Check(BuildAndEval("_or(true, true)"), true);
        Check(BuildAndEval("_or(false, true)"), true);
        Check(BuildAndEval("_or(true, false)"), true);
        Check(BuildAndEval("_or(false, false)"), false);
        Check(BuildAndEval("_or(not(true), not(false))"), true);
    }

    [Fact]
    void Shortcuts()
    {
        throw new NotImplementedException();
        // TODO broken right now, investigate at some point
        // check(buildAndEval("-3"), -3);
        // check(buildAndEval("!true"), false);
        // check(buildAndEval("!false"), true);
        // check(buildAndEval("!(true)"), false);
        // check(buildAndEval("!1"), false);
        // check(buildAndEval("!0"), true);

        // TODO implement
        // check(buildAndEval("true and true"), true);
        // check(buildAndEval("true and false"), false);
        // check(buildAndEval("true or true"), true);
        // check(buildAndEval("true or false"), true);
    }

    [Fact]
    void Conversion()
    {
        Check(BuildAndEval("to_bool(0)"), false);
        Check(BuildAndEval("to_bool(1)"), true);
        Check(BuildAndEval("to_int(true)"), 1);
    }

    [Fact]
    void Equality()
    {
        void f(string co, bool trueValue)
        {
            Check(BuildAndEval(co + "(1, 1)"), trueValue);
            Check(BuildAndEval(co + "(1, 2)"), !trueValue);
            Check(BuildAndEval(co + "(1, true)"), trueValue);
            Check(BuildAndEval(co + "(true, 1)"), trueValue);
            Check(BuildAndEval(co + "(true, 0)"), !trueValue);
            Check(BuildAndEval(co + "('hi', 1)"), !trueValue);
            Check(BuildAndEval(co + "('hi', 'hi')"), trueValue);
            Check(BuildAndEval(co + "('hi', 'hI')"), !trueValue);
            Check(BuildAndEval(co + "('hi', 'bye')"), !trueValue);
            Check(BuildAndEval(co + "(true, true)"), trueValue);
        }

        f("eq", true);
        f("ne", false);
    }

    [Fact]
    void Conditional()
    {
        Check(BuildAndEval("if_else(true, 'hi', 'bye')"), "hi");
        Check(BuildAndEval("if_else(false, 'hi', 'bye')"), "bye");
        Check(BuildAndEval("if_else(1, 'hi', 'bye')"), "hi");
        Check(BuildAndEval("if_else(0, 'hi', 'bye')"), "bye");

        // TODO broken right now, investigate at some point
        // check(buildAndEval("if_else(eq('hi','hi'), 1 + 3, 8)"), 4);
        // check(buildAndEval("if_else(eq('hi','bye'), 1 + 3, 8)"), 8);
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
