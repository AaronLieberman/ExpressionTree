using System;
using System.Xml;

// ReSharper disable once CheckNamespace
namespace Core.Linq;

public static class XmlExtensions
{
    public static T? GetAttributeValue<T>(this XmlReader reader, string name, T? defaultValue = default)
    {
        var result = defaultValue;
        var valueString = reader.GetAttribute(name);

        if (valueString != null)
        {
            result = (T)Convert.ChangeType(valueString, typeof(T));
        }

        return result;
    }
}