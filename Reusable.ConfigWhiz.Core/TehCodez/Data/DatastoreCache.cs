using System.Collections.Generic;

namespace Reusable.SmartConfig.Data
{
    public class DatastoreCache
    {
        private readonly IDictionary<IIdentifier, IDatastore> _datastores = new Dictionary<IIdentifier, IDatastore>();

        public bool TryGetDatastore(IIdentifier identifier, out IDatastore datastore)
        {
            return _datastores.TryGetValue(identifier, out datastore);
        }

        public void Add(IIdentifier id, IDatastore datastore)
        {
            _datastores.Add(id, datastore);
        }

        public void Remove(IIdentifier id)
        {
            _datastores.Remove(id);
        }

        public bool Contains(IIdentifier id) => _datastores.ContainsKey(id);
    }
}