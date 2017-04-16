using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Reusable.Data.Annotations;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public class SettingContainer
    {
        private readonly object _container;
        private readonly IImmutableList<SettingProxy> _proxies;

        public SettingContainer(object container, ContainerPath path, IImmutableList<SettingProxy> proxies)
        {
            _container = container;
            _proxies = proxies;
            Path = path;
        }

        public ContainerPath Path { get; }

        public TContainer As<TContainer>() => (TContainer)_container;

        public IImmutableList<Result<SettingProxy, bool>> Load(LoadOption loadOption)
        {
            return (from p in _proxies select p.Load(loadOption)).ToImmutableList();
        }

        public IImmutableList<Result<SettingProxy, bool>> Save()
        {
            return (from p in _proxies select p.Save()).ToImmutableList();
        }

        public static SettingContainer Create<TConsumer, TContainer>(string containerName, IImmutableList<IDatastore> stores) where TContainer : new()
        {
            var container = new TContainer();
            var containerKey = ContainerPath.Create<TConsumer, TContainer>(containerName);

            var converter =
                typeof(TContainer)
                    .GetCustomAttributes<TypeConverterAttribute>()
                    .Aggregate(
                        Configuration.DefaultConverter,
                        (current, next) => current.Add(next.ConverterType));

            var properties =
                from property in typeof(TContainer).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where property.GetCustomAttribute<IgnoreAttribute>() == null
                select property;

            var proxies =
                from property in properties
                select new SettingProxy(container, containerKey, property, stores, converter);

            return new SettingContainer(container, containerKey, proxies.ToImmutableList());
        }
    }
}