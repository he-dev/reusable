using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Services
{
    public class SettingReader
    {
        private readonly IEnumerable<IDatastore> _datastores;

        public SettingReader(IEnumerable<IDatastore> datastores)
        {
            _datastores = datastores;
        }

        //public Action<string> Log { get; set; }

        public IEntity Read(IEnumerable<CaseInsensitiveString> names)
        {
            return
                _datastores
                    .Select(datastore => datastore.Read(names))
                    .FirstOrDefault(Conditional.IsNotNull) ?? throw new DatastoreNotFoundException(names);
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
        public DatastoreNotFoundException(IEnumerable<CaseInsensitiveString> names)
            : base($"Could not find [{string.Join(", ", names.Select(n => n.ToString()))}]")
        { }
    }
}