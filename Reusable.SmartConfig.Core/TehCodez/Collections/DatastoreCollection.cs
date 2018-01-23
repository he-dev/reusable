using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Collections
{
    public class DatastoreCollection : ICollection<ISettingDataStore>
    {
        private readonly IList<ISettingDataStore> _datastores;

        public DatastoreCollection()
        {
            _datastores = new List<ISettingDataStore>();
        }

        public DatastoreCollection([ItemNotNull] [NotNull] IEnumerable<ISettingDataStore> datastores) : this()
        {
            if (datastores == null)
            {
                throw new ArgumentNullException(nameof(datastores));
            }

            foreach (var datastore in datastores)
            {
                Add(datastore);
            }
        }

        public int Count => _datastores.Count;

        public bool IsReadOnly => _datastores.IsReadOnly;

        public void Add([NotNull] ISettingDataStore item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _datastores.Add(item);
        }

        public void AddRange(IEnumerable<ISettingDataStore> datastores)
        {
            foreach (var datastore in datastores)
            {
                Add(datastore);
            }
        }

        public void Clear()
        {
            _datastores.Clear();
        }

        public bool Contains(ISettingDataStore item)
        {
            return _datastores.Contains(item);
        }

        public void CopyTo(ISettingDataStore[] array, int arrayIndex)
        {
            _datastores.CopyTo(array, arrayIndex);
        }

        public bool Remove(ISettingDataStore item)
        {
            return _datastores.Remove(item);
        }

        public IEnumerator<ISettingDataStore> GetEnumerator()
        {
            return _datastores.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }        
    }
}