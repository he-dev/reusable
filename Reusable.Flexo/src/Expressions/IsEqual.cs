using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    // Makes sure that all types that support a comparer are consistent.
    public interface IFilter : IIdentifiable
    {
        IExpression? Matcher { get; set; }
    }

    public static class FilterExtensions
    {
        public static IEqualityComparer<object> GetEqualityComparer(this IFilter filter, IImmutableContainer context)
        {
            return filter.Matcher switch
            {
                IConstant c => c.Value switch
                {
                    string s => context.GetEqualityComparerOrDefault(s),
                    _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id.ToString()}' filter's '{nameof(IFilter.Matcher)}' must be a comparer-id.")
                },
                _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id.ToString()}' filter must specify a '{nameof(IFilter.Matcher)}' as a comparer-id.")
            };
        }

        public static bool Equal(this IFilter filter, IImmutableContainer context)
        {
            return filter.Matcher switch
            {
                //IConstant _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id.ToString()}' filter's '{nameof(IFilter.Matcher)}' must be a predicate."),
                IConstant c => context.GetEqualityComparerOrDefault().Equals(((IConstant)context.GetItem(ExpressionContext.ThisOuter)).Value, c.Value),
                {} p => p.Invoke(context).Value<bool>(),
                _ => throw new ArgumentException(paramName: nameof(filter), message: $"'{filter.Id.ToString()}' filter must specify a '{nameof(IFilter.Matcher)}' as a predicate.")
            };
        }
    }

    public class IsEqual : ScalarExtension<bool>, IFilter
    {
        public IsEqual() : base(default)
        {
            Matcher = Constant.FromValue("Comparer", "Default");
        }

        public IExpression Left
        {
            get => ThisInner;
            set => ThisInner = value;
        }

        public IExpression Value { get; set; }

        [JsonProperty("Comparer")]
        public IExpression Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var x = This(context).Invoke(context).Value;
            var y = Value.Invoke(context).Value;
            var c = this.GetEqualityComparer(context);
            return c.Equals(x, y);
        }
    }
}