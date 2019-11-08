using System;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : ScalarExtension<object>, IFilter
    {
        public IIf() : base(default)
        {
            Matcher = Constant.FromValue("Comparer", "Default");
        }

        public IExpression Value { get => ThisInner; set => ThisInner = value; }
        
        [JsonProperty("Comparer")]
        public IExpression Matcher { get; set; }
        
        public IExpression True { get; set; }

        public IExpression False { get; set; }

        protected override object ComputeValue(IImmutableContainer context)
        {
            if (True is null && False is null) throw new InvalidOperationException($"You need to specify at least one result ({nameof(True)}/{nameof(False)}).");
            
            var value = This(context).Invoke(context);
            var comparer = this.GetEqualityComparer(context);
            return comparer.Equals(value.Value, true) switch
            {
                true => True?.Invoke(context).Value ?? Constant.Unit,
                false => False?.Invoke(context).Value ?? Constant.Unit
            };
        }

    }
}