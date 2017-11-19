using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Reusable.Collections
{
    /// <summary>
    /// This class generates the two methos of the 'IEquatable' interface. 
    /// It uses the 'EqualityPropertyAttribute' to find the properties that are used to generate both methods.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AutoEquality<T> : IEqualityComparer<T>
    {
        private delegate Expression CreateEqualityComparerMemberExpressionFunc(
            PropertyInfo property,
            AutoEqualityPropertyAttribute attribute,
            Expression leftParameter,
            Expression rightParameter
        );

        // ReSharper disable once InconsistentNaming - cannot rename _comparer to Comparer because it'll conflict with the property.
        private static readonly Lazy<IEqualityComparer<T>> _comparer = new Lazy<IEqualityComparer<T>>(Create);

        private readonly Func<T, T, bool> _equals;
        private readonly Func<T, int> _getHashCode;

        private AutoEquality(Func<T, T, bool> equals, Func<T, int> getHashCode)
        {
            _equals = equals;
            _getHashCode = getHashCode;
        }

        public static IEqualityComparer<T> Comparer => _comparer.Value;

        private static IEqualityComparer<T> Create()
        {
            var leftObjParameter = Expression.Parameter(typeof(T), "leftObj");
            var rightObjParameter = Expression.Parameter(typeof(T), "rightObj");

            var createEqualsExpressionFunc = (CreateEqualityComparerMemberExpressionFunc)AutoEqualityExpressionFactory.CreateEqualsExpression<object>;
            var genericCreateEqualsExpressionMethodInfo = createEqualsExpressionFunc.GetMethodInfo().GetGenericMethodDefinition();

            var createGetHashCodeExpressionFunc = (CreateEqualityComparerMemberExpressionFunc)AutoEqualityExpressionFactory.CreateGetHashCodeExpression<object>;
            var genericCreateGetHashCodeExpressionMethodInfo = createGetHashCodeExpressionFunc.GetMethodInfo().GetGenericMethodDefinition();

            var equalityProperties =
                from property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                let equalityPropertyAttribute = property.GetCustomAttribute<AutoEqualityPropertyAttribute>()
                where equalityPropertyAttribute != null
                let equalsMethod = genericCreateEqualsExpressionMethodInfo.MakeGenericMethod(property.PropertyType)
                let getHashCodeMethod = genericCreateGetHashCodeExpressionMethodInfo.MakeGenericMethod(property.PropertyType)
                let parameters = new object[] { property, equalityPropertyAttribute, leftObjParameter, rightObjParameter }
                select
                (
                    EqualsExpression: (Expression)equalsMethod.Invoke(null, parameters),
                    GetHashCodeExpression: (Expression)getHashCodeMethod.Invoke(null, parameters)
                );

            var equalityComparer = equalityProperties.Aggregate((next, current) =>
            (
                EqualsExpression: Expression.AndAlso(current.EqualsExpression, next.EqualsExpression),
                GetHashCodeExpression: Expression.Add(Expression.Multiply(current.GetHashCodeExpression, Expression.Constant(31)), next.GetHashCodeExpression)
            ));

            var equalsFunc = Expression.Lambda<Func<T, T, bool>>(equalityComparer.EqualsExpression, leftObjParameter, rightObjParameter).Compile();
            var getHashCodeFunc = Expression.Lambda<Func<T, int>>(equalityComparer.GetHashCodeExpression, leftObjParameter).Compile();

            return new AutoEquality<T>(equalsFunc, getHashCodeFunc);
        }

        public bool Equals(T left, T right)
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
}
