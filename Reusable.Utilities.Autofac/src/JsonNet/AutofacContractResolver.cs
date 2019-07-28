using System;
using Autofac;
using Newtonsoft.Json.Serialization;

namespace Reusable.Utilities.Autofac.JsonNet
{
    public class AutofacContractResolver : DefaultContractResolver
    {
        private readonly IComponentContext _container;

        public AutofacContractResolver(IComponentContext context)
        {
            _container = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected override JsonObjectContract CreateObjectContract(Type objectType)
        {
            var contract = base.CreateObjectContract(objectType);

            // use Autofac to create types that have been registered with it
            if (_container.IsRegistered(objectType))
            {
                contract.DefaultCreator = () => _container.Resolve(objectType);
            }

            return contract;
        }
    }
}
