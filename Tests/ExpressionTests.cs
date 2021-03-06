using System;
using System.Collections.Generic;
using System.Diagnostics;
using Core.Linq;
using Xunit;

#pragma warning disable IDE0051

namespace ExpressionTree.Tests;

public class ExpressionTests
{
    static void Check<T>(object? o, T v) where T : notnull
    {
        Assert.IsAssignableFrom<T>(o);
        if (o is T) Assert.Equal(v, o);
    }

    static object? BuildAndEval(string expression, IEvalContext? propertyResolver = null)
    {
        return ExpressionTree.Build(expression).Evaluate(propertyResolver ?? new EvalContext());
    }

    static void BuildAndEvalFailure(string expression, IEvalContext? propertyResolver = null)
    {
        Assert.Throws<InvalidOperationException>(() => BuildAndEval(expression, propertyResolver));
    }

    #region FakeEvalContext

    class FakeEvalContext : IEvalContext
    {
        readonly Dictionary<string, Dictionary<string, object?>> _values = new();

        public void Add(string dataContextName, string propertyName, object? value)
        {
            if (!_values.ContainsKey(dataContextName)) _values[dataContextName] = new Dictionary<string, object?>();
            _values[dataContextName][propertyName] = value;
        }

        public void Add(string dataContextName, string subDataContextName, string propertyName, object? value)
        {
            if (!_values.ContainsKey(dataContextName)) _values[dataContextName] = new Dictionary<string, object?>();
            if (!_values[dataContextName].ContainsKey(subDataContextName)) _values[dataContextName][subDataContextName] = new Dictionary<string, object?>();
            _values[dataContextName][subDataContextName]!.To<Dictionary<string, object?>>()[propertyName] = value;
        }

        public IDictionary<string, object?>? TryGetObject(string propertyName)
        {
            return _values.TryGetValue(propertyName, out Dictionary<string, object?>? dictionary) ? dictionary : null;
        }
    }

    #endregion

    [Fact]
    void Constants()
    {
        Check(BuildAndEval("3"), 3);
        Check(BuildAndEval("3.1"), 3.1f);

        Check(BuildAndEval("\'hello\'"), "hello");
        Check(BuildAndEval("\"hello\""), "hello");

        Check(BuildAndEval("true"), true);
        Check(BuildAndEval("false"), false);
        Check(BuildAndEval("0"), 0);
        Assert.Null(BuildAndEval("null"));
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
    void EvalFailure()
    {
        BuildAndEvalFailure("_test_throw");
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
        Check(BuildAndEval("not(0)"), true);

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
        Check(BuildAndEval("true -and true"), true);
        Check(BuildAndEval("false -and true"), false);
        Check(BuildAndEval("true -and false"), false);
        Check(BuildAndEval("false -and false"), false);
        Check(BuildAndEval("!true -and !false"), false);
        Check(BuildAndEval("true -or true"), true);
        Check(BuildAndEval("false -or true"), true);
        Check(BuildAndEval("true -or false"), true);
        Check(BuildAndEval("false -or false"), false);
        Check(BuildAndEval("!true -or !false"), true);

        Check(BuildAndEval("false && _test_throw"), false);
        BuildAndEvalFailure("true && _test_throw");
        Check(BuildAndEval("true || _test_throw"), true);
        BuildAndEvalFailure("false || _test_throw");
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
        F("==", true);
        F("-ne", false);
        F("!=", false);
    }

    [Fact]
    void SimpleFunction()
    {
        Assert.InRange(Convert.ToSingle(BuildAndEval("sin(1.57)")), .9, 1.1);
    }

    [Fact]
    void Conditional()
    {
        Check(BuildAndEval("if_else(true, 'hi', 'bye')"), "hi");
        Check(BuildAndEval("if_else(false, 'hi', 'bye')"), "bye");
        Check(BuildAndEval("if_else(1, 'hi', 'bye')"), "hi");
        Check(BuildAndEval("if_else(0, 'hi', 'bye')"), "bye");

        Check(BuildAndEval("if_else('hi' -eq 'hi', 1 + 3, 8)"), 4);
        Check(BuildAndEval("if_else('hi' -eq 'bye', 1 + 3, 8)"), 8);

        Check(BuildAndEval("if_else(true, 4, _test_throw)"), 4);
        Check(BuildAndEval("if_else(false, _test_throw, 5)"), 5);
        BuildAndEvalFailure("if_else(false, 4, _test_throw)");

        BuildAndEvalFailure("if_else(3 -gt 3, 6, _test_throw)");
        BuildAndEvalFailure("if_else(3 > 3, 6, _test_throw)");
        Check(BuildAndEval("if_else(3 -ge 3, 6, _test_throw)"), 6);
        Check(BuildAndEval("if_else(3 >= 3, 6, _test_throw)"), 6);
    }

    [Fact]
    void Properties()
    {
        var resolver = new FakeEvalContext();
        resolver.Add("context", "value_3", 3);
        resolver.Add("context", "value_here", "here");
        resolver.Add("context", "value_true", true);
        resolver.Add("context", "value_false", false);
        resolver.Add("context", "object", "foo", "bar");

        Check(BuildAndEval("context.value_3", resolver), 3);
        Check(BuildAndEval("context.value_true", resolver), true);
        Check(BuildAndEval("context.value_false", resolver), false);
        Check(BuildAndEval("not_null(context.value_3)", resolver), true);
        Assert.Null(BuildAndEval("context.missing", resolver));
        Check(BuildAndEval("not_null(context.missing)", resolver), false);
        Check(BuildAndEval("exists_else(context.value_3, 4)", resolver), 3);
        Check(BuildAndEval("exists_else(context.missing, 5)", resolver), 5);

        Check(BuildAndEval("exists_else(context.value_here, 'nope')", resolver), "here");
        Check(BuildAndEval("exists_else(context.missing, 'nope')", resolver), "nope");

        Check(BuildAndEval("!(context.value_true)", resolver), false);
        Check(BuildAndEval("!(context.value_false)", resolver), true);
        Check(BuildAndEval("!context.value_true", resolver), false);

        // identifiers must be literals
        BuildAndEvalFailure("context.if_else(true, object, none).foo", resolver);
    }

    [Fact]
    void CaseSensitivity()
    {
        var resolver = new FakeEvalContext();
        resolver.Add("context", "value_3", 3);
        resolver.Add("context", "value_true", true);
        resolver.Add("context", "value_false", false);

        Check(BuildAndEval("TrUe", resolver), true);
        Check(BuildAndEval("FaLsE", resolver), false);
        Assert.Null(BuildAndEval("nUll", resolver));

        Check(BuildAndEval("noT(false)", resolver), true);

        Check(BuildAndEval("cOnTeXt.value_3", resolver), 3);
        Check(BuildAndEval("context.VaLuE_3", resolver), 3);
        Check(BuildAndEval("context.VaLuE_true", resolver), true);
        Check(BuildAndEval("context.VaLuE_false", resolver), false);
        Assert.Null(BuildAndEval("context.MiSsInG", resolver));
    }
}
