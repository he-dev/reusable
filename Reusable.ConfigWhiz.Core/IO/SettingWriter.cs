using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Reusable.ConfigWhiz.Converters;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Paths;
using Reusable.Extensions;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz.IO
{
    public class SettingWriter
    {
        private readonly DatastoreCache _datastoreCache;
        private readonly SettingConverter _converter;

        public SettingWriter(DatastoreCache datastoreCache, SettingConverter converter)
        {
            _datastoreCache = datastoreCache;
            _converter = converter;
        }

        public int Write(SettingContainer container)
        {
            var innerExceptions = new List<Exception>();

            var settingsAffected = 0;

            foreach (var setting in container)
            {
                try
                {
                    if (_datastoreCache.TryGetDatastore(setting.Id, out var datastore))
                    {
                        var entities = _converter.Serialize(setting, datastore.SupportedTypes);
                        settingsAffected += entities.Count();
                        datastore.Write(entities);
                    }
                    else
                    {
                        // todo throw datastore not initialized
                    }
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
    }
}