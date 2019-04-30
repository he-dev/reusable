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

        protected override Constant<TResult> InvokeCore(IImmutableSession context, IExpression @this)
        {
            //var @this = context.PopThisConstant().Value<object>();
            var value = @this.Invoke(context).Value;
            var switchContext = context.Set(Use<ISwitchSession>.Scope, x => x.Value, value);

            foreach (var switchCase in (Cases ?? Enumerable.Empty<SwitchCase>()).Where(c => c.Enabled))
            {
                switch (switchCase.When)
                {
                    case IConstant constant:
                        if (EqualityComparer<object>.Default.Equals(value, constant.Value))
                        {
                            var bodyResult = switchCase.Body.Invoke(context);
                            return (Name, (TResult)bodyResult.Value, bodyResult.Context);
                        }

                        break;

                    case IExpression expression:
                        if (expression.Invoke(switchContext) is var whenResult && whenResult.Value<bool>())
                        {
                            var bodyResult = switchCase.Body.Invoke(context);
                            return (Name, (TResult)bodyResult.Value, bodyResult.Context);
                        }

                        break;
                }
            }

            // todo - make it dynamic-exception
            return
                Default is null
                    ? throw new ArgumentOutOfRangeException()
                    : (Name, Default.Invoke(context).Value<TResult>(), context);

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

    public interface ISwitchSession : ISession
    {
        object Value { get; }
    }
}