using Autofac;
using Newtonsoft.Json.Serialization;

namespace Reusable.Utilities.Autofac.JsonNet
{
    public class JsonContractResolverModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c =>
                {
                    var context = c.Resolve<IComponentContext>();
                    return new AutofacContractResolver(context);
                })
                .SingleInstance()
                .As<IContractResolver>();
        }
    }
}