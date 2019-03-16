using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Switch : Expression
    {
        public Switch() : base(nameof(Switch)) { }

        public IExpression Value { get; set; }

        //[JsonRequired]
        public List<SwitchCase> Cases { get; set; } = new List<SwitchCase>();

        //[JsonRequired]
        public IExpression Default { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            var value = context.Get(Item.For<IExtensionContext>(), x => x.Input) ?? Value;
            var switchContext = context.Set(Item.For<ISwitchContext>(), x => x.Value, value);

            foreach (var switchCase in Cases.Where(c => c.Enabled))
            {
                var when =
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


                if (when.Invoke(switchContext).Value<bool>())
                {
                    return switchCase.Body.Invoke(context);
                }
            }

            return Default.Invoke(context);
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

    public interface ISwitchContext
    {
        object Value { get; }
    }
}