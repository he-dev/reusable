using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.TypeConversion;

namespace Reusable.SmartConfig
{
    public class ConfigurationBuilder
    {
        private readonly List<IDatastore> _datastores = new List<IDatastore>();

        private TypeConverter _converter = Configuration.DefaultConverter;

        [NotNull]
        public ConfigurationBuilder WithDatastore<T>([NotNull] T datastore) where T : IDatastore
        {
            if (datastore == null) throw new ArgumentNullException(nameof(datastore));
            _datastores.Add(datastore);
            return this;
        }

        [NotNull]
        public ConfigurationBuilder WithDatastores<T>([NotNull, ItemNotNull] IEnumerable<T> datastores) where T : IDatastore
        {
            if (datastores == null) throw new ArgumentNullException(nameof(datastores));
            _datastores.AddRange(datastores.Cast<IDatastore>());
            return this;
        }

        [NotNull]
        public ConfigurationBuilder WithConverter<T>() where T : TypeConverter, new()
        {
            _converter = _converter.Add<T>();            
            return this;
        }

        [NotNull]
        public IConfiguration Build()
        {
            return new Configuration(_datastores, _converter);
        }
    }
}