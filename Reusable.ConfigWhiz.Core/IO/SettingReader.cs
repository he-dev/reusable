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
            if (cached && _datastoreCache.Contains(container.Id))
            {
                return container;
            }

            var innerExceptions = new List<Exception>();

            foreach (var setting in container)
            {
                try
                {
                    var entities = Read(setting.Id);
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

        private ICollection<IEntity> Read(IIdentifier id)
        {
            return
                _datastoreCache.TryGetDatastore(id, out var datastore) 
                    ? datastore.Read(id) 
                    : Resolve(id);
        }

        private ICollection<IEntity> Resolve(IIdentifier id)
        {
            foreach (var datastore in _datastores)
            {
                var settings = datastore.Read(id);
                if (settings.Any())
                {
                    _datastoreCache.Remove(id);
                    _datastoreCache.Add(id, datastore);
                    return settings;
                }
            }

            throw new DatastoreNotFoundException(id);
        }
    }

    public class UnsupportedItemizedTypeException : Exception
    {
        public UnsupportedItemizedTypeException(IIdentifier id, Type settingType)
            : base($"'{settingType}' type used by '{id}' setting is not supported for itemized settings. You can use either {nameof(IDictionary)} or {nameof(IEnumerable)}.")
        { }
    }

    public class MultipleSettingMatchesException : Exception
    {
        public MultipleSettingMatchesException(IIdentifier id, IDatastore datastore)
            : base($"Found multiple matches of '{id}' in '{datastore.Name}'  but expected one.")
        { }
    }

    public class DatastoreNotFoundException : Exception
    {
        public DatastoreNotFoundException(IIdentifier id)
            : base($"Could not find datastore for '{id}'")
        { }
    }
}