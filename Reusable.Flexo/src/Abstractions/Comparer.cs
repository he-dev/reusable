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
            //get => Arg;
            set => Arg = value;
        }

        [JsonRequired]
        [JsonProperty("Value")]
        public IExpression Right { get; set; } = default!;

        public string? ComparerName { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var comparer = this.GetComparer(context);
            var x = GetArg(context).FirstOrDefault();
            var y = Right.Invoke(context).Cast<object>().FirstOrDefault();
            var result = comparer.Compare(x, y);
            return _predicate(result);
        }
    }
}