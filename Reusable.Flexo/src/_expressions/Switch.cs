using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    [UsedImplicitly]
    [PublicAPI]
    public class Switch : Expression
    {
        public Switch() : base(nameof(Switch)) { }

        [JsonRequired]
        public IExpression Value { get; set; }

        //[JsonRequired]
        public List<SwitchCase> Cases { get; set; } = new List<SwitchCase>();

        //[JsonRequired]
        public IExpression Default { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            var switchContext =
                context
                    .Set(Item.For<ISwitchContext>(), x => x.Value, Value);

            foreach (var switchCase in Cases)
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