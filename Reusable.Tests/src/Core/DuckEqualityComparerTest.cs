using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Reflection;

namespace Reusable.Tests
{
    using static Assert;
    using static ExpressionHelper;

    [TestClass]
    public class DuckEqualityComparerTest
    {
        [TestMethod]
        public void Main()
        {
            var p1 = new PersonLib1 { FirstName = "John", LastName = "Doe" };
            var p2 = new PersonLib2 { FirstName = "JOHN", LastName = "Doe" };
            var p3 = new PersonLib2 { FirstName = "Joh", LastName = "Doe" };

            var stringComparer = StringComparer.OrdinalIgnoreCase;
            var personComparer = DuckEqualityComparer<PersonLib1, PersonLib2>.Create(
                equals: (x, y) =>
                    stringComparer.Equals(x.FirstName, y.FirstName) &&
                    stringComparer.Equals(x.LastName, y.LastName),
                getHashCodeX: x =>
                    stringComparer.GetHashCode(x.FirstName) +
                    stringComparer.GetHashCode(x.LastName),
                getHashCodeY: y =>
                    stringComparer.GetHashCode(y.FirstName) +
                    stringComparer.GetHashCode(y.LastName)
            );

            //	personComparer.Equals(p1, p1).Dump();
            //	personComparer.Equals(p2, p2).Dump();
            //
            //	personComparer.Equals(p1, p2).Dump();
            //	personComparer.Equals(p2, p1).Dump();
            //
            //	personComparer.Equals(p1, p3).Dump();
            //	personComparer.Equals(p3, p1).Dump();


            var personComparer2 =
                new DuckEqualityComparerBuilder<PersonLib1, PersonLib2>()
                    .Compare(x => x.FirstName, y => y.FirstName, StringComparer.OrdinalIgnoreCase)
                    .Compare(x => x.LastName, y => y.LastName, StringComparer.OrdinalIgnoreCase)
                    .Build();

            //personComparer2.Equals(p1, p1).Dump();
            //personComparer2.Equals(p2, p2).Dump();

            IsTrue(personComparer2.Equals(p1, p2));
            IsTrue(personComparer2.Equals(p2, p1));

            IsFalse(personComparer2.Equals(p1, p3));
            IsFalse(personComparer2.Equals(p3, p1));

            var anonymousComparer =
                DuckEqualityComparerBuilder
                    .Create(
                        new { FirstName1 = default(string), LastName1 = default(string) },
                        new { FirstName2 = default(string), LastName2 = default(string) }
                    )
                    .Compare(x => x.FirstName1, y => y.FirstName2, StringComparer.OrdinalIgnoreCase)
                    .Compare(x => x.LastName1, y => y.LastName2, StringComparer.OrdinalIgnoreCase)
                    .Build();

            IsTrue(anonymousComparer.Equals(
                new { FirstName1 = "John", LastName1 = "Doe" },
                new { FirstName2 = "JOHN", LastName2 = "DOE" }
            ));
        }
    }

    public class DuckEqualityComparerBuilder
    {
        public static DuckEqualityComparerBuilder<TX, TY> Create<TX, TY>(TX x, TY y) => new DuckEqualityComparerBuilder<TX, TY>();
    }

    public class DuckEqualityComparerBuilder<TX, TY>
    {
        private readonly ParameterExpression _parameterX = Expression.Parameter(typeof(TX), "x");
        private readonly ParameterExpression _parameterY = Expression.Parameter(typeof(TY), "y");

        private readonly IList<(Expression equals, Expression getHashCodeX, Expression getHashCodeY)> _expressions = new List<(Expression, Expression, Expression)>();

        public DuckEqualityComparerBuilder<TX, TY> Compare<T>(
            Expression<Func<TX, T>> getValueX,
            Expression<Func<TY, T>> getValueY,
            IEqualityComparer<T> comparer = null
        )
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

        public IEqualityComparer<object> Build()
        {
            var equalityComparer = _expressions.Aggregate((next, current) =>
            (
                equals: ConcatenateEqualsExpressions(current.equals, next.equals),
                getHashCodeX: ConcatenateGetHashCodeExpressions(current.getHashCodeX, next.getHashCodeX),
                getHashCodeY: ConcatenateGetHashCodeExpressions(current.getHashCodeY, next.getHashCodeY)
            ));

            var equalsFunc = Expression.Lambda<Func<TX, TY, bool>>(equalityComparer.equals, _parameterX, _parameterY).Compile();
            var getHashCodeXFunc = Expression.Lambda<Func<TX, int>>(equalityComparer.getHashCodeX, _parameterX).Compile();
            var getHashCodeYFunc = Expression.Lambda<Func<TY, int>>(equalityComparer.getHashCodeY, _parameterY).Compile();

            return DuckEqualityComparer<TX, TY>.Create(
                equalsFunc,
                getHashCodeXFunc,
                getHashCodeYFunc
            );
        }
    }

    internal static class ExpressionHelper
    {
        public static Expression ConcatenateEqualsExpressions(Expression equalsExpressionX, Expression equalsExpressionY)
        {
            // equals && equals
            return Expression.AndAlso(equalsExpressionX, equalsExpressionY);
        }

        public static Expression ConcatenateGetHashCodeExpressions(Expression getHashCodeExpressionX, Expression getHashCodeExpressionY)
        {
            // x * 31 + y
            return Expression.Add(Expression.Multiply(getHashCodeExpressionX, Expression.Constant(31)), getHashCodeExpressionY);
        }
    }

    public class DuckEqualityComparer<TX, TY> : IEqualityComparer<object>
    {
        private readonly Func<TX, TY, bool> _equals;
        private readonly Func<TX, int> _getHashCodeX;
        private readonly Func<TY, int> _getHashCodeY;

        private DuckEqualityComparer(Func<TX, TY, bool> equals, Func<TX, int> getHashCodeX, Func<TY, int> getHashCodeY)
        {
            _equals = equals;
            _getHashCodeX = getHashCodeX;
            _getHashCodeY = getHashCodeY;
        }

        public new bool Equals(object x, object y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            ValidateType(x);
            ValidateType(y);

            if (Equals((x, y))) return true;
            if (Equals((y, x))) return true;
            return false;
        }

        private bool Equals((object x, object y) t)
        {
            return t.x is TX x && t.y is TY y && _equals(x, y);
        }

        public int GetHashCode(object obj)
        {
            ValidateType(obj);

            switch (obj)
            {
                case TX x: return _getHashCodeX(x);
                case TY y: return _getHashCodeY(y);
                default: return 0;
            }
        }

        public static IEqualityComparer<object> Create(Func<TX, TY, bool> equals, Func<TX, int> getHashCodeX, Func<TY, int> getHashCodeY)
        {
            return new DuckEqualityComparer<TX, TY>(equals, getHashCodeX, getHashCodeY);
        }

        private static void ValidateType(object obj)
        {
            if (!(obj is TX || obj is TY))
            {
                throw new ArgumentException($"Type '{obj.GetType().Name}' is not supported. Objects must be '{typeof(TX).Name}' and '{typeof(TY).Name}'");
            }
        }
    }

    class PersonLib1
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    class PersonLib2
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}