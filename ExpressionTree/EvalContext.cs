using System;
using System.Collections.Generic;

namespace ExpressionTree;

public interface IEvalContext
{
    IDictionary<string, object?>? TryGetObject(string propertyName);
}

public class EvalContext : IEvalContext
{
    public IDictionary<string, object?>? TryGetObject(string propertyName)
    {
        throw new NotImplementedException();
    }
}
