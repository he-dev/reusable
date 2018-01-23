using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Collections
{
    public class SettingDataStoreCollection : ICollection<ISettingDataStore>
    {
        private readonly IList<ISettingDataStore> _dataStores;

        public SettingDataStoreCollection()
        {
            _dataStores = new List<ISettingDataStore>();
        }

        public SettingDataStoreCollection([ItemNotNull] [NotNull] IEnumerable<ISettingDataStore> datastores) : this()
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

        public int Count => _dataStores.Count;

        public bool IsReadOnly => _dataStores.IsReadOnly;

        public void Add([NotNull] ISettingDataStore item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _dataStores.Add(item);
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
            _dataStores.Clear();
        }

        public bool Contains(ISettingDataStore item)
        {
            return _dataStores.Contains(item);
        }

        public void CopyTo(ISettingDataStore[] array, int arrayIndex)
        {
            _dataStores.CopyTo(array, arrayIndex);
        }

        public bool Remove(ISettingDataStore item)
        {
            return _dataStores.Remove(item);
        }

        public IEnumerator<ISettingDataStore> GetEnumerator()
        {
            return _dataStores.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }        
    }
}