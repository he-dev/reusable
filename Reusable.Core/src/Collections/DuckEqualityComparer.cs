using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Collections
{
    using static ExpressionHelper;
    
    // https://docs.microsoft.com/en-us/dotnet/api/system.collections.iequalitycomparer.equals?view=netframework-4.7.2#notes-to-implementers
    
    public class DuckEqualityComparer<TX, TY> : EqualityComparer<object>
    {
        private readonly Func<TX, TY, bool> _equals;
        private readonly Func<TX, int> _getHashCodeX;
        private readonly Func<TY, int> _getHashCodeY;
        private readonly bool _isCanonical;

        private DuckEqualityComparer(Func<TX, TY, bool> equals, Func<TX, int> getHashCodeX, Func<TY, int> getHashCodeY, bool isCanonical)
        {
            _equals = equals;
            _getHashCodeX = getHashCodeX;
            _getHashCodeY = getHashCodeY;
            _isCanonical = isCanonical;
        }

        public override bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            ValidateType(x);
            ValidateType(y);

            if (!_isCanonical && !(x is TX && y is TY))
            {
                throw new ArgumentException($"This comparer is not canonical (reflexive, symmetric, and transitive). You have to specify parameters in the following order: {typeof(TX).Name} then {typeof(TY).Name}.");
            }

            if (Equals((x, y))) return true;

            return false;
        }

        private bool Equals((object x, object y) t)
        {
            return t.x is TX x && t.y is TY y && _equals(x, y);
        }

        public override int GetHashCode(object obj)
        {
            ValidateType(obj);

            switch (obj)
            {
                case TX x: return _getHashCodeX(x);
                case TY y: return _getHashCodeY(y);
                default: return 0;
            }
        }

        [NotNull]
        public static EqualityComparer<object> Create
        (
            [NotNull] Func<TX, TY, bool> equals,
            [NotNull] Func<TX, int> getHashCodeX,
            [NotNull] Func<TY, int> getHashCodeY,
            bool isCanonical = true
        )
        {
            if (equals == null) throw new ArgumentNullException(nameof(equals));
            if (getHashCodeX == null) throw new ArgumentNullException(nameof(getHashCodeX));
            if (getHashCodeY == null) throw new ArgumentNullException(nameof(getHashCodeY));

            return new DuckEqualityComparer<TX, TY>(equals, getHashCodeX, getHashCodeY, isCanonical);
        }

        private static void ValidateType(object obj)
        {
            if (!(obj is TX || obj is TY))
            {
                throw new ArgumentException($"Type '{obj.GetType().Name}' is not supported. Objects must be '{typeof(TX).Name}' or '{typeof(TY).Name}'");
            }
        }
    }    

    public class DuckEqualityComparerBuilder<TX, TY>
    {
        private readonly bool _isCanonical;
        private readonly ParameterExpression _parameterX = Expression.Parameter(typeof(TX), "param_x");
        private readonly ParameterExpression _parameterY = Expression.Parameter(typeof(TY), "param_y");

        private readonly IList<(Expression equals, Expression getHashCodeX, Expression getHashCodeY)> _expressions = new List<(Expression, Expression, Expression)>();

        public DuckEqualityComparerBuilder(bool isCanonical = true)
        {
            _isCanonical = isCanonical;
        }

        public DuckEqualityComparerBuilder<TX, TY> Compare<T>(Expression<Func<TX, T>> getValueX, Expression<Func<TY, T>> getValueY, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;

            // comparer.Equals(getValueX(x), getValueY(y));
            var equalsFunc = (Expression<Func<T, T, bool>>)((x, y) => comparer.Equals(x, y));
            var equals =
                Expression.Invoke(
                    equalsFunc,
                    Expression.Invoke(getValueX, _parameterX),
                    Expression.Invoke(getValueY, _parameterY)
                );

            // comparer.GetHashCode(getValueX(x))
            var getHashCodeFunc = (Expression<Func<T, int>>)(obj => comparer.GetHashCode(obj));

            var getHashCodeX =
                Expression.Invoke(
                    getHashCodeFunc,
                    Expression.Invoke(getValueX, _parameterX)
                );

            // comparer.GetHashCode(getValueY(y))
            var getHashCodeY =
                Expression.Invoke(
                    getHashCodeFunc,
                    Expression.Invoke(getValueY, _parameterY)
                );

            _expressions.Add((equals, getHashCodeX, getHashCodeY));

            return this;
        }

        public DuckEqualityComparerBuilder<TX, TY> Compare<T>(Expression<Func<TX, T>> getValueX, Expression<Func<TY, T>> getValueY, Expression<Func<T, T, bool>> equals)
        {
            var label = Expression.Label(typeof(bool));

            var variableX = Expression.Variable(typeof(T), "var_x");
            var variableY = Expression.Variable(typeof(T), "var_y");

            // We need to protect the custom equality-check from null.
            var referenceEquals = (Expression<Func<T, bool>>)(obj => ReferenceEquals(obj, null));

            var equalsGuarded =
                Expression.Block(
                    new[] { variableX, variableY },
                    // x = getValueX(TX);
                    Expression.Assign(variableX, Expression.Invoke(getValueX, _parameterX)),
                    // y = getValueX(TY);
                    Expression.Assign(variableY, Expression.Invoke(getValueY, _parameterY)),
                    // if (ReferenceEquals(x, null) return false;
                    Expression.IfThen(
                        test: Expression.IsTrue(Expression.Invoke(referenceEquals, variableX)),
                        ifTrue: Expression.Return(label, Expression.Constant(false))
                    ),
                    // if (ReferenceEquals(y, null) return false;
                    Expression.IfThen(
                        test: Expression.IsTrue(Expression.Invoke(referenceEquals, variableY)),
                        ifTrue: Expression.Return(label, Expression.Constant(false))
                    ),
                    // return equals(x, y)
                    Expression.Return(label, Expression.Invoke(equals, variableX, variableY)),

                    // This makes the compiler happy. 
                    Expression.Label(label, Expression.Constant(false))
                );

            // With a custom Equals we probably cannot GetHashCode anyway so just skip it.
            // return 0;
            var getHashCodeX = Expression.Constant(0);
            var getHashCodeY = Expression.Constant(0);

            _expressions.Add((equalsGuarded, getHashCodeX, getHashCodeY));

            return this;
        }

        public EqualityComparer<object> Build()
        {
            var equalityComparer = _expressions.Aggregate(
                (next, current) =>
                (
                    equals: ConcatenateEqualsExpressions(current.equals, next.equals),
                    getHashCodeX: ConcatenateGetHashCodeExpressions(current.getHashCodeX, next.getHashCodeX),
                    getHashCodeY: ConcatenateGetHashCodeExpressions(current.getHashCodeY, next.getHashCodeY)
                )
            );

            var equalsFunc = Expression.Lambda<Func<TX, TY, bool>>(equalityComparer.equals, new[] { _parameterX, _parameterY }).Compile();
            var getHashCodeXFunc = Expression.Lambda<Func<TX, int>>(equalityComparer.getHashCodeX, _parameterX).Compile();
            var getHashCodeYFunc = Expression.Lambda<Func<TY, int>>(equalityComparer.getHashCodeY, _parameterY).Compile();

            return DuckEqualityComparer<TX, TY>.Create(equalsFunc, getHashCodeXFunc, getHashCodeYFunc, _isCanonical);
        }

        public static implicit operator EqualityComparer<object>(DuckEqualityComparerBuilder<TX, TY> builder) => builder.Build();
    }
    
    public static class DuckEqualityComparerBuilder
    {
        public static DuckEqualityComparerBuilder<TX, TY> Create<TX, TY>(TX x, TY y) => new DuckEqualityComparerBuilder<TX, TY>();
    }

    internal static class ExpressionHelper
    {
        public static Expression ConcatenateEqualsExpressions(Expression equalsExpressionX, Expression equalsExpressionY)
        {
            // equals && equals
            return
                Expression.AndAlso(
                    equalsExpressionX,
                    equalsExpressionY
                );
        }

        public static Expression ConcatenateGetHashCodeExpressions(Expression getHashCodeX, Expression getHashCodeY)
        {
            // x * 31 + y
            return
                Expression.Add(
                    Expression.Multiply(
                        getHashCodeX,
                        Expression.Constant(31)
                    ),
                    getHashCodeY
                );
        }
    }
}