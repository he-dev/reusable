using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public abstract class Switch<TResult> : Expression<object>, IExtension<TResult>
    {
        protected Switch([NotNull] SoftString name) : base(name) { }

        protected Switch(ILogger logger, SoftString name) : base(logger, name) { }

        public IExpression Value { get; set; }

        public List<SwitchCase> Cases { get; set; } = new List<SwitchCase>();

        public IExpression Default { get; set; }

        protected override Constant<object> InvokeCore(IExpressionContext context)
        {
            var value = context.TryPopExtensionInput(out object input) ? input : Value.Invoke(context).Value;
            var switchContext = context.Set(Item.For<ISwitchContext>(), x => x.Value, value);

            foreach (var switchCase in (Cases ?? Enumerable.Empty<SwitchCase>()).Where(c => c.Enabled))
            {
                switch (switchCase.When)
                {
                    case IConstant constant:
                        if (EqualityComparer<object>.Default.Equals(value, constant.Value))
                        {
                            var bodyResult = switchCase.Body.Invoke(context);
                            return (Name, bodyResult.Value, bodyResult.Context);
                        }

                        break;

                    case IExpression expression:
                        if (expression.Invoke(switchContext) is var whenResult && whenResult.Value<bool>())
                        {
                            var bodyResult = switchCase.Body.Invoke(context);
                            return (Name, bodyResult.Value, bodyResult.Context);
                        }

                        break;
                }
            }


            return
            (
                Name,
                (Default ?? new Throw
                    {
                        Name = "SwitchValueOutOfRange",
                        Message = Constant.FromValue("Message", "Default value not specified.")
                    }
                ).Invoke(context),
                context
            );
        }
    }

    [UsedImplicitly]
    [PublicAPI]
    public class Switch : Switch<IExpression>
    {
        public Switch(string name) : base(name) { }

        public Switch(ILogger<Switch> logger) : base(logger, nameof(Switch)) { }

        public Switch() : this(nameof(Switch)) { }
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

    public interface ISwitchContext
    {
        object Value { get; }
    }
}