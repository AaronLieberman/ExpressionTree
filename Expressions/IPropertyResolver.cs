namespace Expressions;

public interface IPropertyResolver
{
    object? GetPropertyValue(string key, string propertyName);
}