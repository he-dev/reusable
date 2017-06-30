using System;
using System.Collections.Generic;

namespace Reusable.SmartConfig.Collections
{
    public class DatastoreComparer : IEqualityComparer<IDatastore>
    {
        public bool Equals(IDatastore x, IDatastore y)
        {
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            return ReferenceEquals(x, y) || StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name);
        }

        public int GetHashCode(IDatastore obj)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(obj.Name);
        }
    }
}