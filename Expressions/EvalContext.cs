using System;

namespace Expressions;

public interface IEvalContext
{
    object? GetPropertyValue(string propertyName);
}

public class EvalContext : IEvalContext
{
    public object? GetPropertyValue(string propertyName)
    {
        throw new NotImplementedException();
    }
}
