using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;
using Reusable.Quickey;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Switch : ValueExtension<object>
    {
        public Switch(ILogger<Switch> logger) : this(logger, nameof(Switch)) { }
        
        protected Switch(ILogger logger, SoftString name) : base(logger, name) { }

        [JsonProperty("Value")]
        public override IExpression This { get; set; }

        public IEnumerable<SwitchCase> Cases { get; set; }

        public IExpression Default { get; set; }

        protected override Constant<object> InvokeCore(IExpression @this)
        {
            var value = @this.Invoke();

            foreach (var switchCase in (Cases ?? Enumerable.Empty<SwitchCase>()).Where(c => c.Enabled))
            {
                using (BeginScope(ctx => ctx.SetItem(ExpressionContext.This, value)))
                {
                    switch (switchCase.When)
                    {
                        case IConstant constant:
                            if (EqualityComparer<object>.Default.Equals(value.Value, constant.Value))
                            {
                                var bodyResult = switchCase.Body.Invoke();
                                return (Name, bodyResult.Value);
                            }

                            break;
                        case IExpression expression:
                            if (expression.Invoke() is var whenResult && whenResult.Value<bool>())
                            {
                                var bodyResult = switchCase.Body.Invoke();
                                return (Name, bodyResult.Value);
                            }

                            break;
                    }
                }
            }

            if (Default is IConstant @default)
            {
                return ("Switch.Default", @default.Value);
            }

            return
            (
                Name,
                (Default ?? new Throw
                    {
                        Name = "SwitchValueOutOfRange",
                        Message = Constant.FromValue("Message", "Default value not specified.")
                    }
                ).Invoke()
            );
        }
    }

    public class SwitchCase
    {
        [DefaultValue(true)]
        public bool Enabled { get; set; } = true;

        [JsonRequired]
        public IExpression When { get; set; }

        [JsonRequired]
        public IExpression Body { get; set; }
    }

//    [UseType]
//    [UseMember]
//    [TrimEnd("I")]
//    [TrimStart("Meta")]
//    public interface ISwitchMeta : INamespace
//    {
//        object Value { get; }
//    }
}