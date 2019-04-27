using System.ComponentModel.DataAnnotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Reusable.Tests.Flexo
{
    public class ExpressionContextTest
    {
        [Fact]
        public void Can_write_and_read_values()
        {
            var expression = new MyExpression();

            var context = ImmutableSession.Empty.Set(Item.For<MyExpression>(), e => e.Greeting, Constant.FromValue("Greeting", "Hallo!"));
            
            ExpressionAssert.Equal(Constant.FromValue("Expected", "Hallo!"), expression, context);
        }
        
        [Fact]
        public void Throws_when_required_value_not_found()
        {
            var expression = new MyExpression();

            Assert.ThrowsAny<DynamicException>(() => expression.Invoke(ExpressionContext.Empty));
        }

        private class MyExpression : Expression<string>
        {
            public MyExpression() : base(nameof(MyExpression))
            { }

            [Required]
            public IExpression Greeting { get; set; }

            protected override Constant<string> InvokeCore(IImmutableSession context)
            {
                var item = context.Get(Item.For<MyExpression>(), e => e.Greeting).Invoke(context);
                return (Name, (string)item.Value, item.Context);
            }
        }
    }
}