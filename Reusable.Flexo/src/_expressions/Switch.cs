using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.Flexo
{
    public abstract class Switch<TResult> : Expression<object>, IExtension<TResult>
    {
        protected Switch([NotNull] SoftString name) : base(name) { }

        public IExpression Value { get; set; }

        public List<SwitchCase> Cases { get; set; } = new List<SwitchCase>();

        public IExpression Default { get; set; }

        protected override Constant<object> InvokeCore(IExpressionContext context)
        {
            var value = ExtensionInputOrDefault(ref context, Value).Invoke(context).Value;
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
                        //var switchCaseContext = switchContext.Set(Item.For<ISwitchContext>(), x => x.Case, switchCase..Value);
                        var whenResult = expression.Invoke(switchContext);
                        if ((bool)whenResult.Value)
                        {
                            var bodyResult = switchCase.Body.Invoke(context);
                            return (Name, bodyResult.Value, bodyResult.Context);
                        }
                        break;
                }                
            }

            return (Name, (Default ?? new Throw("SwitchValueOutOfRange") { Message = Constant.FromValue("Message", "Default value not specified.") }).Invoke(context), context);
            
        }
    }

    [UsedImplicitly]
    [PublicAPI]
    public class Switch : Switch<IExpression> // Expression<IExpression>, IExtension<IExpression>
    {
        public Switch(string name) : base(name) { }

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
            return switchCase.When;
//                switchCase.When is IConstant constant
//                    ? new ObjectEqual
//                    {
//                        Left = new GetContextItem
//                        {
//                            Key = ExpressionContext.CreateKey(Item.For<ISwitchContext>(), x => x.Value)
//                        },
//                        Right = constant
//                    }
//                    : switchCase.When;
        }
    }

    public interface ISwitchContext
    {
        object Value { get; }
        
        object Case { get; }
    }
}