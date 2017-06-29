using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Paths;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz.IO
{
    public class SettingReader
    {
        private readonly IEnumerable<IDatastore> _datastores;
        private readonly DatastoreCache _datastoreCache;
        private readonly SettingConverter _converter;

        public SettingReader(DatastoreCache datastoreCache, SettingConverter converter, IEnumerable<IDatastore> datastores)
        {
            _datastores = datastores;
            _datastoreCache = datastoreCache;
            _converter = converter;
        }

        public SettingContainer Read(SettingContainer container, bool cached)
        {
            if (cached && _datastoreCache.Contains(container.Identifier))
            {
                return container;
            }

            var innerExceptions = new List<Exception>();

            foreach (var setting in container)
            {
                try
                {
                    var entities = Read(setting.Identifier);
                    setting.Value = _converter.Deserialize(setting, entities);
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

            return container;
        }

        private ICollection<IEntity> Read(Identifier identifier)
        {
            return
                _datastoreCache.TryGetDatastore(identifier, out var datastore) 
                    ? datastore.Read(identifier) 
                    : Resolve(identifier);
        }

        private ICollection<IEntity> Resolve(Identifier identifier)
        {
            foreach (var datastore in _datastores)
            {
                var settings = datastore.Read(identifier);
                if (settings.Any())
                {
                    _datastoreCache.Remove(identifier);
                    _datastoreCache.Add(identifier, datastore);
                    return settings;
                }
            }

            throw new DatastoreNotFoundException(identifier);
        }
    }

    public class UnsupportedItemizedTypeException : Exception
    {
        public UnsupportedItemizedTypeException(Identifier identifier, Type settingType)
            : base($"'{settingType}' type used by '{identifier}' setting is not supported for itemized settings. You can use either {nameof(IDictionary)} or {nameof(IEnumerable)}.")
        { }
    }

    public class MultipleSettingMatchesException : Exception
    {
        public MultipleSettingMatchesException(Identifier identifier, IDatastore datastore)
            : base($"Found multiple matches of '{identifier}' in '{datastore.Name}'  but expected one.")
        { }
    }

    public class DatastoreNotFoundException : Exception
    {
        public DatastoreNotFoundException(Identifier identifier)
            : base($"Could not find datastore for '{identifier}'")
        { }
    }
}