using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Expressions;

public interface IEvalContext
{
    object? GetPropertyValue(string key, string propertyName);
}

public class EvalContext : IEvalContext
{
    public object? GetPropertyValue(string key, string propertyName)
    {
        throw new NotImplementedException();
    }
}
