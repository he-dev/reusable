using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public abstract class Switch<TResult> : ValueExtension<TResult>
    {
        protected Switch(ILogger logger, SoftString name) : base(logger, name) { }

        [JsonProperty("Value")]
        public override IExpression This { get; set; }

        public IEnumerable<SwitchCase> Cases { get; set; }

        public IExpression Default { get; set; }

        protected override Constant<TResult> InvokeCore(IExpression @this)
        {
            var value = @this.Invoke();

            foreach (var switchCase in (Cases ?? Enumerable.Empty<SwitchCase>()).Where(c => c.Enabled))
            {
                using (BeginScope(ctx => ctx.Set(Namespace, x => x.This, value)))
                {
                    switch (switchCase.When)
                    {
                        case IConstant constant:
                            if (EqualityComparer<object>.Default.Equals(value.Value, constant.Value))
                            {
                                var bodyResult = switchCase.Body.Invoke();
                                return (Name, (TResult)bodyResult.Value);
                            }

                            break;

                        case IExpression expression:
                            if (expression.Invoke() is var whenResult && whenResult.Value<bool>())
                            {
                                var bodyResult = switchCase.Body.Invoke();
                                return (Name, (TResult)bodyResult.Value);
                            }

                            break;
                    }
                }
            }

            // todo - make it dynamic-exception
            return
                Default is null
                    ? throw new ArgumentOutOfRangeException()
                    : (Name, Default.Invoke().Value<TResult>());

            // return
            // (
            //     Name,
            //     (Default ?? new Throw
            //         {
            //             Name = "SwitchValueOutOfRange",
            //             Message = Constant.FromValue("Message", "Default value not specified.")
            //         }
            //     ).Invoke(context),
            //     context
            // );
        }
    }

    [UsedImplicitly]
    [PublicAPI]
    public class Switch : Switch<object>
    {
        public Switch(ILogger<Switch> logger) : base(logger, nameof(Switch)) { }
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

    public interface ISwitchNamespace : INamespace
    {
        object Value { get; }
    }
}