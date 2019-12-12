using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    // Makes sure that all types that support a comparer are consistent.
    public interface IFilter : IIdentifiable
    {
        IExpression? Matcher { get; set; }
    }

    public static class Filter
    {
        public static class Properties
        {
            public const string Comparer = nameof(Comparer);

            public const string Predicate = nameof(Predicate);
        }
    }

    public static class FilterExtensions
    {
        public static IEqualityComparer<object> GetEqualityComparer(this IFilter filter, IImmutableContainer context)
        {
            return filter.Matcher switch
            {
                IConstant c => c.Value<object>() switch
                {
                    string s => context.FindItem(ExpressionContext.EqualityComparers, s),
                    _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id}' filter's '{nameof(IFilter.Matcher)}' must be a comparer-id.")
                },
                _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id}' filter must specify a '{nameof(IFilter.Matcher)}' as a comparer-id.")
            };
        }

        public static bool Equal(this IFilter filter, IConstant x, IImmutableContainer context)
        {
            return filter.Matcher switch
            {
                //IConstant c => context.FindItem(ExpressionContext.EqualityComparers, "Default").Equals(x.Single(), c.Single()),
                IConstant c => x.Cast<object>().SequenceEqual(c.Cast<object>(), context.FindItem(ExpressionContext.EqualityComparers, "Default")),
                {} p => p.Invoke(context.BeginScopeWithArg(x)).Cast<bool>().All(b => b),
                _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id}' filter must specify a '{nameof(IFilter.Matcher)}' as a predicate.")
            };
        }
    }

    public class IsEqual : Extension<object, bool>, IFilter
    {
        public IsEqual() : base(default)
        {
            Matcher = Constant.DefaultComparer;
        }

        public IExpression? Left
        {
            set => Arg = value;
        }

        [JsonRequired]
        public IExpression Value { get; set; } = default!;

        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression? Matcher { get; set; }

        protected override bool ComputeSingle(IImmutableContainer context)
        {
            var x = GetArg(context);
            var y = Value.Invoke(context).Cast<object>();
            var c = this.GetEqualityComparer(context);
            return x.SequenceEqual(y, c);
        }
    }
}