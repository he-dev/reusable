using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;

namespace Reusable
{
    internal enum CompareOperator
    {
        LessThan,
        Equal,
        GreaterThan
    }

    internal static class ComparerFactory<T>
    {
        private class Comparer : IComparer<T>
        {
            private readonly IDictionary<CompareOperator, Func<T, T, bool>> _comparers;

            internal Comparer([NotNull] IDictionary<CompareOperator, Func<T, T, bool>> comparers)
            {
                _comparers = comparers;
            }

            public int Compare(T x, T y)
            {
                if (ReferenceEquals(x, y)) return 0;
                if (ReferenceEquals(x, null)) return -1;
                if (ReferenceEquals(y, null)) return 1;

                if (_comparers[CompareOperator.LessThan](x, y)) return -1;
                if (_comparers[CompareOperator.Equal](x, y)) return 0;
                if (_comparers[CompareOperator.GreaterThan](x, y)) return 1;

                // Makes the compiler very happy.
                return 0;
            }
        }

        public static IComparer<T> Create<TComparable>(Expression<Func<T, TComparable>> selectComparable, Action<ComparerBuilder<T, TComparable>, TComparable, TComparable> create)
        {
            var builder = new ComparerBuilder<T, TComparable>(selectComparable);
            create(builder, default, default);
            var funcs = builder.Build();
            return new Comparer(funcs);
        }
    }

    internal class ComparerBuilder<T, TComparable>
    {
        private readonly Expression<Func<T, TComparable>> _getComparable;
        private readonly IDictionary<CompareOperator, Expression<Func<bool>>> _expressions = new Dictionary<CompareOperator, Expression<Func<bool>>>();

        public ComparerBuilder(Expression<Func<T, TComparable>> getComparable)
        {
            _getComparable = getComparable;
        }

        public ComparerBuilder<T, TComparable> LessThen(Expression<Func<bool>> expression)
        {
            _expressions[CompareOperator.LessThan] = expression;
            return this;
        }

        public ComparerBuilder<T, TComparable> Equal(Expression<Func<bool>> expression)
        {
            _expressions[CompareOperator.Equal] = expression;
            return this;
        }

        public ComparerBuilder<T, TComparable> GreaterThan(Expression<Func<bool>> expression)
        {
            _expressions[CompareOperator.GreaterThan] = expression;
            return this;
        }

        internal IDictionary<CompareOperator, Func<T, T, bool>> Build()
        {
            var left = Expression.Parameter(typeof(T), "left");
            var right = Expression.Parameter(typeof(T), "right");

            return _expressions.ToDictionary(x => x.Key, x => CompileComparer(x.Value, new[] { left, right }));
        }

        private Func<T, T, bool> CompileComparer(Expression compare, ParameterExpression[] parameters)
        {
            var rewritten = CompareRewriter.Rewrite(_getComparable, parameters, compare);
            var lambda = Expression.Lambda<Func<T, T, bool>>(Expression.Invoke(rewritten), parameters);
            return lambda.Compile();
        }
    }

    internal class CompareRewriter : ExpressionVisitor
    {
        private readonly Expression _getComparable;
        private readonly ParameterExpression[] _parameters;
        private int _param;

        public CompareRewriter(Expression getComparable, ParameterExpression[] parameters)
        {
            _getComparable = getComparable;
            _parameters = parameters;
        }

        public static Expression Rewrite(Expression getComparable, ParameterExpression[] parameters, Expression compare)
        {
            var visitor = new CompareRewriter(getComparable, parameters);
            return visitor.Visit(compare);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            var binary = Visit((BinaryExpression)node.Body);
            return Expression.Lambda<Func<bool>>(binary);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.Equal) return base.VisitBinary(node);

            // Rewrite
            // () => ClosureT1.x < ClosureT2.y
            // to
            // () => getComparable(T2).x < getComparable(T2).y

            var getLeft = Expression.Invoke(_getComparable, _parameters[0]);
            var getRight = Expression.Invoke(_getComparable, _parameters[1]);

            _param = 0;
            var left = Visit(node.Left);
            _param++;
            var right = Visit(node.Right);

            // Determine whether a member-access is necessary or are we using pure values?
            left = left == node.Left ? getLeft : left;
            right = right == node.Right ? getRight : right;

            switch (node.NodeType)
            {
                case ExpressionType.LessThan: return Expression.LessThan(left, right);
                case ExpressionType.Equal: return Expression.Equal(left, right);
                case ExpressionType.GreaterThan: return Expression.GreaterThan(left, right);
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return
                node.Member.MemberType == MemberTypes.Property
                ? Expression.MakeMemberAccess(Expression.Invoke(_getComparable, _parameters[_param]), node.Member)
                : base.VisitMember(node);
        }
    }
}