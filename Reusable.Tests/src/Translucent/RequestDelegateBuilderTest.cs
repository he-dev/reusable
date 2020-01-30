using System.Threading.Tasks;
using Reusable.Translucent.Data;
using Xunit;

namespace Reusable.Translucent
{
    public class RequestDelegateBuilderTest
    {
        // [Fact]
        // public void Can_build_pipeline()
        // {
        //     var builder = new PipelineBuilder<Context>();
        //
        //     var invoke = builder.UseMiddleware<M1>().UseMiddleware<M1>().UseMiddleware<M1>().Build(ImmutableServiceProvider.Empty);
        //     var context = new Context();
        //     invoke(context);
        //     Assert.Equal(3, context.Counter);
        // }
        
        [Fact]
        public void Can_build_pipeline2()
        {
            var invoke = PipelineFactory.CreatePipeline<Context>(new []
            {
                MiddlewareInfo<Context>.Create<M1>(),
                MiddlewareInfo<Context>.Create<M1>(),
                MiddlewareInfo<Context>.Create<M1>(),
            }, ImmutableServiceProvider.Empty);
            
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