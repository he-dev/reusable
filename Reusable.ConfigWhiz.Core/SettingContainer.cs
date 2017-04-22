using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.TypeConversion;
using TypeConverterAttribute = Reusable.TypeConversion.TypeConverterAttribute;

namespace Reusable.ConfigWhiz
{
    public abstract class SettingContainer
    {
        protected SettingContainer(object value, ContainerPath path, IImmutableList<SettingProxy> proxies)
        {
            Value = value;
            Path = path;
            Proxies = proxies;
        }

        protected IImmutableList<SettingProxy> Proxies { get; }
        public ContainerPath Path { get; }
        public object Value { get; }
        public abstract void Load(LoadOption loadOption);
        public abstract int Save();
    }

    public class SettingContainer<TContainer> : SettingContainer where TContainer : new()
    {
        protected SettingContainer(TContainer value, ContainerPath path, IImmutableList<SettingProxy> proxies)
            : base(value, path, proxies)
        { }

        public override void Load(LoadOption loadOption)
        {
            var innerExceptions = new List<Exception>();

            foreach (var proxy in Proxies)
            {
                try
                {
                    proxy.Load(loadOption);
                }
                catch (DatastoreReadException)
                {
                    // Cannot continue if datastore fails to read.
                    throw;
                }
                catch (Exception ex)
                {
                    innerExceptions.Add(ex);
                }
            }

            if (innerExceptions.Any())
            {
                throw new AggregateException("Could not read one or more settings. See inner exceptions for details.", innerExceptions);
            }
        }

        public override int Save()
        {
            var innerExceptions = new List<Exception>();

            var settingsAffected = 0;
            foreach (var proxy in Proxies)
            {
                try
                {
                    settingsAffected += proxy.Save();
                }
                catch (DatastoreWriteException)
                {
                    // Cannot continue if datastore fails to write.
                    throw;
                }
                catch (Exception ex)
                {
                    innerExceptions.Add(ex);
                }
            }

            if (innerExceptions.Any())
            {
                throw new AggregateException("Could not write one or more settings. See inner exceptions for details.", innerExceptions);
            }

            return settingsAffected;
        }

        public static SettingContainer Create<TConsumer>(string containerName, IImmutableList<IDatastore> stores)
        {
            var container = new TContainer();
            var containerKey = ContainerPath.Create<TConsumer, TContainer>(containerName);

            var converter =
                typeof(TContainer)
                    .GetCustomAttributes<TypeConverterAttribute>()
                    .Aggregate(
                        Configuration.DefaultConverter,
                        (current, next) => current.Add(next.ConverterType));

            var proxies =
                from property in typeof(TContainer).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where property.GetCustomAttribute<IgnoreAttribute>().IsNull()
                select new SettingProxy(container, containerKey, property, stores, converter);

            return new SettingContainer<TContainer>(container, containerKey, proxies.ToImmutableList());
        }

        public static implicit operator TContainer(SettingContainer<TContainer> settingContainer) => (TContainer)settingContainer.Value;
    }
}