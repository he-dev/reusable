using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using Reusable.Marbles.Extensions;

namespace Reusable.Marbles.Collections;

/// <summary>
/// This class generates the two methods of the 'IEquatable' interface. 
/// It uses the 'EqualityPropertyAttribute' to find the properties that are used to generate both methods.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AutoEquality<T> : IEqualityComparer<T>
{
    private delegate Expression CreateEqualityComparerMemberExpressionFunc(AutoEqualityPropertyContext context);

    // ReSharper disable once InconsistentNaming - cannot rename _comparer to Comparer because it'll conflict with the property.
    private static readonly Lazy<IEqualityComparer<T>> _comparer = new(Create);

    private readonly Func<T, T, bool> _equals;
    private readonly Func<T, int> _getHashCode;

    private AutoEquality(Func<T, T, bool> equals, Func<T, int> getHashCode)
    {
        _equals = equals;
        _getHashCode = getHashCode;
    }

    public static IEqualityComparer<T> Comparer => _comparer.Value;

    public static AutoEqualityBuilder<T> Builder => new();

    private static IEqualityComparer<T> Create()
    {
        var equalityProperties =
        (
            from property in typeof(T).GetPropertiesMany(BindingFlags.Public | BindingFlags.Instance)
            where property.IsDefined(typeof(AutoEqualityPropertyAttribute))
            let attribute = property.GetCustomAttribute<AutoEqualityPropertyAttribute>()
            select (property, attribute)
        ).ToList();

        if (equalityProperties.Empty())
        {
            throw DynamicException.Create("AutoEqualityPropertyNotFound", $"Could not find any '{nameof(AutoEqualityPropertyAttribute)}' on '{typeof(T).ToPrettyString()}'");
        }

        return Create(equalityProperties);
    }

    internal static IEqualityComparer<T> Create(IEnumerable<(PropertyInfo Property, AutoEqualityPropertyAttribute Attribute)> equalityProperties)
    {
        //var objParameters = equalityProperties.Select(ep => Expression.Parameter(ep.Property.PropertyType, ep.Property.Name)).ToList();
        var leftParameter = Expression.Parameter(typeof(T), "left");
        var rightParameter = Expression.Parameter(typeof(T), "right");

        var createEqualsExpressionFunc = (CreateEqualityComparerMemberExpressionFunc)AutoEqualityExpressionFactory.CreateEqualsExpression<object>;
        var genericCreateEqualsExpressionMethodInfo = createEqualsExpressionFunc.GetMethodInfo().GetGenericMethodDefinition();

        var createGetHashCodeExpressionFunc = (CreateEqualityComparerMemberExpressionFunc)AutoEqualityExpressionFactory.CreateGetHashCodeExpression<object>;
        var genericCreateGetHashCodeExpressionMethodInfo = createGetHashCodeExpressionFunc.GetMethodInfo().GetGenericMethodDefinition();

        var equalityExpressions =
            from item in equalityProperties
            let createEqualsExpressionMethod = genericCreateEqualsExpressionMethodInfo.MakeGenericMethod(item.Property.PropertyType)
            let createGetHashCodeMethod = genericCreateGetHashCodeExpressionMethodInfo.MakeGenericMethod(item.Property.PropertyType)
            let parameters = new object[]
            {
                new AutoEqualityPropertyContext
                {
                    Property = item.Property,
                    Attribute = item.Attribute,
                    LeftParameter = leftParameter,
                    RightParameter = rightParameter
                }
            }
            select
            (
                EqualsExpression: (Expression)createEqualsExpressionMethod.Invoke(null, parameters),
                GetHashCodeExpression: (Expression)createGetHashCodeMethod.Invoke(null, parameters)
            );

        var equalityComparer = equalityExpressions.Aggregate((next, current) =>
        (
            EqualsExpression: Expression.AndAlso(current.EqualsExpression, next.EqualsExpression),
            GetHashCodeExpression: Expression.Add(Expression.Multiply(current.GetHashCodeExpression, Expression.Constant(31)), next.GetHashCodeExpression)
        ));

        var equalsFunc = Expression.Lambda<Func<T, T, bool>>(equalityComparer.EqualsExpression, leftParameter, rightParameter).Compile();
        var getHashCodeFunc = Expression.Lambda<Func<T, int>>(equalityComparer.GetHashCodeExpression, leftParameter).Compile();

        return new AutoEquality<T>(equalsFunc, getHashCodeFunc);
    }

    public bool Equals(T? left, T? right)
    {
        if (ReferenceEquals(left, right)) return true;
        if (ReferenceEquals(left, null)) return false;
        if (ReferenceEquals(right, null)) return false;
        return _equals(left, right);
    }

    public int GetHashCode(T obj)
    {
        return _getHashCode(obj);
    }               
}