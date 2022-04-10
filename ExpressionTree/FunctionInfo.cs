using System;
using System.Diagnostics;

namespace ExpressionTree;

enum Associativity { Left, Right }

[DebuggerDisplay("{Name,nq} (args={ArgCount})")]
struct FunctionInfo
{
    public string Name { get; }
    public int Precidence { get; }
    public Associativity Associativity { get; }
    public int ArgCount { get; }
    public Func<IEvalContext, ExpressionTree[], object?> Evaluate { get; }

    public FunctionInfo(string name, int precidence, Associativity associativity, int argCount,
        Func<IEvalContext, ExpressionTree[], object?> evaluate)
    {
        Name = name;
        Precidence = precidence;
        Associativity = associativity;
        ArgCount = argCount;
        Evaluate = evaluate;
    }
}