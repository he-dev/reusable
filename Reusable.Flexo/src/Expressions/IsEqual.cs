using System.Collections.Generic;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    // Makes sure that all types that support a comparer are consistent.
    public interface IFilter
    {
        IExpression? Predicate { get; set; }

        [JsonProperty("Comparer")]
        string? ComparerName { get; set; }
    }

    public static class FilterExtensions
    {
        public static IEqualityComparer<object> GetEqualityComparer(this IFilter filter, IImmutableContainer context)
        {
            return context.GetEqualityComparerOrDefault(filter.ComparerName);
        }

        public static bool Equal(this IFilter filter, IImmutableContainer context, IConstant x, object y)
        {
            var comparer = filter.GetEqualityComparer(context);
            
            return filter.Predicate switch
            {
                IConstant constant => comparer.Equals(x.Value, constant.Value),
                { } predicate => predicate.Invoke(context.BeginScopeWithThisOuter(x)).Value<bool>(),
                _ => comparer.Equals(x.Value, y)
            };
        }
    }

    public class IsEqual : ScalarExtension<bool>, IFilter
    {
        public IsEqual() : base(default, nameof(IsEqual)) { }

        public IExpression Left
        {
            get => ThisInner;
            set => ThisInner = value;
        }

        [JsonProperty("Value")]
        public IExpression? Predicate { get; set; }

        public string? ComparerName { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var x = This(context).Invoke(context);
            return this.Equal(context, x, default);
        }
    }
}