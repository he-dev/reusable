using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public abstract class Switch<TResult> : Expression<IExpression>, IExtension<TResult>
    {
        protected Switch([NotNull] SoftString name, [NotNull] IExpressionContext context) : base(name, context) { }

        public IExpression Value { get; set; }

        public List<SwitchCase> Cases { get; set; } = new List<SwitchCase>();

        public IExpression Default { get; set; }

        protected override ExpressionResult<IExpression> InvokeCore(IExpressionContext context)
        {
            var value = ExtensionInputOrDefault(ref context, Value);
            var switchContext = context.Set(Item.For<ISwitchContext>(), x => x.Value, value);

            foreach (var switchCase in (Cases ?? Enumerable.Empty<SwitchCase>()).Where(c => c.Enabled))
            {
                if (switchCase.WhenOrDefault().Invoke(switchContext).Value<bool>())
                {
                    return (switchCase.Body.Invoke(switchContext), context);
                }
            }

            return ((Default ?? new Throw("SwitchValueOutOfRange") { Message = Constant.FromValue("Message", "Default value not specified.") }).Invoke(context), context);
        }
    }

    [UsedImplicitly]
    [PublicAPI]
    public class Switch : Switch<IExpression> // Expression<IExpression>, IExtension<IExpression>
    {
        public Switch(string name) : base(name, ExpressionContext.Empty) { }

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

    public static class SwitchCaseExtensions
    {
        public static IExpression WhenOrDefault(this SwitchCase switchCase)
        {
            return
                switchCase.When is IConstant constant
                    ? new ObjectEqual
                    {
                        Left = new GetContextItem
                        {
                            Key = ExpressionContext.CreateKey(Item.For<ISwitchContext>(), x => x.Value)
                        },
                        Right = constant
                    }
                    : switchCase.When;
        }
    }

    public interface ISwitchContext
    {
        object Value { get; }
    }
}