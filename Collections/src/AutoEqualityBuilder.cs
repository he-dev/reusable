using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Reusable.Collections;

public class AutoEqualityBuilder<T>
{
    private readonly List<(PropertyInfo Property, AutoEqualityPropertyAttribute Attribute)> _equalityProperties;

    internal AutoEqualityBuilder()
    {
        _equalityProperties = new List<(PropertyInfo Property, AutoEqualityPropertyAttribute Attribute)>();
    }

    public AutoEqualityBuilder<T> Use<TProperty>(Expression<Func<T, TProperty>> expression)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException(message: "Expression must be a property.", paramName: nameof(expression));
        }

        _equalityProperties.Add(((PropertyInfo)memberExpression.Member, new AutoEqualityPropertyAttribute()));
        return this;
    }

    public AutoEqualityBuilder<T> Use(Expression<Func<T, string>> expression, StringComparison stringComparison)
    {
        if (expression.Body is not MemberExpression memberExpression)
        {
            throw new ArgumentException(message: "Expression must be a property.", paramName: nameof(expression));
        }

        _equalityProperties.Add(((PropertyInfo)memberExpression.Member, new AutoEqualityPropertyAttribute(stringComparison)));
        return this;
    }

    public IEqualityComparer<T> Build()
    {
        return AutoEquality<T>.Create(_equalityProperties);
    }
}