using System.ComponentModel.DataAnnotations;
using Reusable.Exceptionizer;
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

            var context = ExpressionContext.Empty.Set(Item.For<MyExpression>(), e => e.Greeting, "Hallo!");
            
            Helpers.Equal(Constant.Create("Expected", "Hallo!"), expression, context);
        }
        
        [Fact]
        public void Throws_when_requried_value_not_found()
        {
            var expression = new MyExpression();

            Assert.ThrowsAny<DynamicException>(() => expression.Invoke(ExpressionContext.Empty));
        }

        private class MyExpression : Expression
        {
            public MyExpression() : base(nameof(MyExpression))
            { }

            [Required]
            public string Greeting { get; set; }

            public override IExpression Invoke(IExpressionContext context)
            {
                var name = context.Get(Item.For<MyExpression>(), e => e.Greeting);
                return Constant.Create("Actual", name);
            }
        }
    }
}