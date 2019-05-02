using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.Data;

namespace Reusable.Flexo
{
    public static class Expression
    {
        // ReSharper disable RedundantNameQualifier - Use full namespace to avoid conflicts with other types.
        public static readonly Type[] Types =
        {
            typeof(Reusable.Flexo.IsEqual),
            typeof(Reusable.Flexo.IsGreaterThan),
            typeof(Reusable.Flexo.IsGreaterThanOrEqual),
            typeof(Reusable.Flexo.IsLessThan),
            typeof(Reusable.Flexo.IsLessThanOrEqual),
            typeof(Reusable.Flexo.Not),
            typeof(Reusable.Flexo.All),
            typeof(Reusable.Flexo.Any),
            typeof(Reusable.Flexo.IIf),
            typeof(Reusable.Flexo.Switch),
            typeof(Reusable.Flexo.ToDouble),
            typeof(Reusable.Flexo.ToString),
            typeof(Reusable.Flexo.GetValue),
            typeof(Reusable.Flexo.GetCollection),
            typeof(Reusable.Flexo.Ref),
            typeof(Reusable.Flexo.Contains),
            typeof(Reusable.Flexo.In),
            typeof(Reusable.Flexo.Overlaps),
            typeof(Reusable.Flexo.Matches),
            typeof(Reusable.Flexo.Min),
            typeof(Reusable.Flexo.Max),
            typeof(Reusable.Flexo.Count),
            typeof(Reusable.Flexo.Sum),
            typeof(Reusable.Flexo.Constant<>),
            typeof(Reusable.Flexo.Double),
            typeof(Reusable.Flexo.Integer),
            typeof(Reusable.Flexo.Decimal),
            typeof(Reusable.Flexo.DateTime),
            typeof(Reusable.Flexo.TimeSpan),
            typeof(Reusable.Flexo.String),
            typeof(Reusable.Flexo.True),
            typeof(Reusable.Flexo.False),
            typeof(Reusable.Flexo.Collection),
            typeof(Reusable.Flexo.Select),
            typeof(Reusable.Flexo.Throw),
        };
        // ReSharper restore RedundantNameQualifier

        public static IImmutableSession DefaultSession =>
            ImmutableSession
                .Empty
                .Set(Use<IExpressionSession>.Scope, x => x.This, new Stack<IConstant>())
                .Set(Use<IExpressionSession>.Scope, x => x.Comparers, ImmutableDictionary<SoftString, IEqualityComparer<object>>.Empty)
                .Set(Use<IExpressionSession>.Scope, x => x.DebugView, TreeNode.Create(ExpressionDebugView.Root))
                .WithDefaultComparer()
                .WithSoftStringComparer()
                .WithRegexComparer();
    }
}