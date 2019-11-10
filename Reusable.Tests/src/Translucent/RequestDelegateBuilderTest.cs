using System.Threading.Tasks;
using Xunit;

namespace Reusable.Translucent
{
    public class RequestDelegateBuilderTest
    {
        [Fact]
        public void Can_build_pipeline()
        {
            var builder = new RequestDelegateBuilder<Context>(ImmutableServiceProvider.Empty);

            var invoke = builder.UseMiddleware<M1>().UseMiddleware<M1>().UseMiddleware<M1>().Build();
            var context = new Context();
            invoke(context);
            Assert.Equal(3, context.Counter);
        }
        
        private class M1
        {
            private readonly RequestDelegate<Context> _next;

            public M1(RequestDelegate<Context> next) => _next = next;

            public Task Invoke(Context context)
            {
                _next(context);
                context.Counter++;
                return Task.CompletedTask;
            }
        }

        private class Context
        {
            public int Counter { get; set; }
        }
    }
}