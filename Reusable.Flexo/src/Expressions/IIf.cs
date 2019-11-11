using System;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : ScalarExtension<object>, IFilter
    {
        public IIf() : base(default)
        {
            Matcher = Constant.DefaultComparer;
        }

        public IExpression? Value { get => Arg; set => Arg = value; }
        
        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression? Matcher { get; set; }
        
        public IExpression? True { get; set; }

        public IExpression? False { get; set; }

        protected override object ComputeValue(IImmutableContainer context)
        {
            if (True is null && False is null) throw new InvalidOperationException($"You need to specify at least one result ({nameof(True)}/{nameof(False)}).");
            
            var value = GetArg(context).Invoke(context);
            var comparer = this.GetEqualityComparer(context);
            return comparer.Equals(value.Value!, true) switch
            {
                true => True?.Invoke(context).Value ?? Constant.Unit,
                false => False?.Invoke(context).Value ?? Constant.Unit
            };
        }

    }
}