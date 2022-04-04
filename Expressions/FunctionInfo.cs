﻿using System;
using System.Diagnostics;

namespace Expressions;

[DebuggerDisplay("{Name,nq} (args={ArgCount})")]
struct FunctionInfo
{
    public string Name { get; }
    public int Precidence { get; }
    public Associativity Associativity { get; }
    public int ArgCount { get; }
    public Func<object?[], object?> Evaluate { get; }

    public FunctionInfo(string name, int precidence, Associativity associativity, int argCount, Func<object?[], object?> evaluate)
    {
        Name = name;
        Precidence = precidence;
        Associativity = associativity;
        ArgCount = argCount;
        Evaluate = evaluate;
    }
}