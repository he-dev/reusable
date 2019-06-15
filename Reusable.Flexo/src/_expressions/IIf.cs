using System;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : ValueExtension<object>
    {
        public IIf(ILogger<IIf> logger) : base(logger, nameof(IIf)) { }

        [JsonProperty("Predicate")]
        public override IExpression This { get; set; }

        public IExpression True { get; set; }

        public IExpression False { get; set; }

        protected override Constant<object> InvokeCore(IExpression @this)
        {
            if (True is null && False is null) throw new InvalidOperationException($"You need to specify at least one result ({nameof(True)}/{nameof(False)}).");

            var value = @this.Invoke();

            if (value.Value<bool>())
            {
                var trueResult = True?.Invoke();
                return (Name, trueResult?.Value ?? Constant.Null);
            }
            else
            {
                var falseResult = False?.Invoke();
                return (Name, falseResult?.Value ?? Constant.Null);
            }
        }
    }
}