using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Reusable.ConfigWhiz.Paths;
using Reusable.Data.Annotations;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz
{
    public abstract class SettingContainer
    {
        protected SettingContainer(object instance, Identifier identifier, IImmutableList<SettingProxy> proxies)
        {
            Value = instance;
            Identifier = identifier;
            Proxies = proxies;
        }

        protected IImmutableList<SettingProxy> Proxies { get; }
        public Identifier Identifier { get; }
        public object Value { get; }
        public abstract void Load();
        public abstract int Save();
    }

    public class SettingContainer<TContainer> : SettingContainer where TContainer : class, new()
    {
        protected SettingContainer(TContainer instance, Identifier identifier, IImmutableList<SettingProxy> proxies)
            : base(instance, identifier, proxies)
        { }

        public override void Load()
        {
            var innerExceptions = new List<Exception>();

            foreach (var proxy in Proxies)
            {
                try
                {
                    proxy.Load();
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
            foreach (var proxy in Proxies.Where(p => !p.ReadOnly))
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

        public static SettingContainer<TContainer> Create(Identifier identifier, IImmutableList<IDatastore> datastores)
        {
            var container = new TContainer();

            var converter =
                typeof(TContainer)
                    .GetCustomAttributes<Reusable.TypeConversion.TypeConverterAttribute>()
                    .Aggregate(
                        Configuration.DefaultConverter,
                        (current, next) => current.Add(next.ConverterType));

            var proxies =
                from property in typeof(TContainer).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where property.GetCustomAttribute<IgnoreAttribute>().IsNull()
                select new SettingProxy(Identifier.From(identifier, property.Name), container, property, datastores, converter);

            return new SettingContainer<TContainer>(container, identifier, proxies.ToImmutableList());
        }

        public static implicit operator TContainer(SettingContainer<TContainer> settingContainer) => (TContainer)settingContainer.Value;
    }
}