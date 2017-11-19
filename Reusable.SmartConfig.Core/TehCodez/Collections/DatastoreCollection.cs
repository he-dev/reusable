using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Collections
{
    public class DatastoreCollection : ICollection<IDatastore>
    {
        private readonly IList<IDatastore> _datastores;

        public DatastoreCollection()
        {
            _datastores = new List<IDatastore>();
        }

        public DatastoreCollection([ItemNotNull] [NotNull] IEnumerable<IDatastore> datastores) : this()
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

        public void Add([NotNull] IDatastore item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            _datastores.Add(item);
        }

        public void AddRange(IEnumerable<IDatastore> datastores)
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

        public bool Contains(IDatastore item)
        {
            return _datastores.Contains(item);
        }

        public void CopyTo(IDatastore[] array, int arrayIndex)
        {
            _datastores.CopyTo(array, arrayIndex);
        }

        public bool Remove(IDatastore item)
        {
            return _datastores.Remove(item);
        }

        public IEnumerator<IDatastore> GetEnumerator()
        {
            return _datastores.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }        
    }
}