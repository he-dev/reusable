using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo.Abstractions
{
    // Makes sure that all types that support a comparer are consistent.
    public interface ISorter
    {
        [JsonProperty("Comparer")]
        string? ComparerName { get; set; }
    }

    public static class SorterExtensions
    {
        public static IComparer<object> GetComparer(this ISorter sorter, IImmutableContainer context)
        {
            return context.FindItem(ExpressionContext.Comparers, sorter.ComparerName ?? Keywords.Default);
        }
    }

    [PublicAPI]
    public abstract class Comparer : Extension<object, bool>, ISorter
    {
        private readonly Func<int, bool> _predicate;

        protected Comparer(ILogger? logger, Func<int, bool> predicate)
            : base(logger) => _predicate = predicate;

        public IExpression? Left
        {
            set => Arg = value;
        }

        [JsonRequired]
        [JsonProperty("Value")]
        public IExpression Right { get; set; } = default!;

        public string? ComparerName { get; set; }

        protected override IEnumerable<bool> ComputeMany(IImmutableContainer context)
        {
            var x = GetArg(context);
            var y = Right.Invoke(context);
            var c = this.GetComparer(context);
            //var result = c.Compare(x, y);
            //return _predicate(result);
            return x.Zip(y, (a, b) => c.Compare(a, b)).Select(r => _predicate(r));
        }
    }
}