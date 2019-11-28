using System;
using System.Collections.Generic;
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
                IConstant c => c.Value switch
                {
                    string s => context.FindItem(ExpressionContext.EqualityComparers, s ?? "Default"),
                    _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id}' filter's '{nameof(IFilter.Matcher)}' must be a comparer-id.")
                },
                _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id}' filter must specify a '{nameof(IFilter.Matcher)}' as a comparer-id.")
            };
        }

        public static bool Equal(this IFilter filter, IImmutableContainer context, IConstant x)
        {
            return filter.Matcher switch
            {
                IConstant c => context.FindItem(ExpressionContext.EqualityComparers, "Default").Equals(x.Value!, c.Value!),
                {} p => p.Invoke(context.BeginScopeWithArg(x)).Value<bool>(),
                _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id}' filter must specify a '{nameof(IFilter.Matcher)}' as a predicate.")
            };
        }
    }

    public class IsEqual : ScalarExtension<bool>, IFilter
    {
        public IsEqual() : base(default)
        {
            Matcher = Constant.DefaultComparer;
        }

        public IExpression? Left
        {
            get => Arg;
            set => Arg = value;
        }

        [JsonRequired]
        public IExpression Value { get; set; } = default!;

        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression? Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var x = GetArg(context).Invoke(context).Value;
            var y = Value.Invoke(context).Value;
            var c = this.GetEqualityComparer(context);
            return c.Equals(x!, y!);
        }
    }
}