using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Logging.ComputedProperties;

namespace Reusable.Logging.Collections
{
    public class ComputedPropertyCollection : IEnumerable<IComputedProperty>
    {
        private readonly HashSet<IComputedProperty> _properties = new HashSet<IComputedProperty>(new ComputedPropertyComparer());

        public void Add(IComputedProperty property)
        {
            if (!_properties.Add(property)) throw new InvalidOperationException($"Cannot add '{property.GetType().Name}' because another property with the name '{property.Name}' already exists.");
        }
        
        public ComputedPropertyCollection Add(string name, Func<LogEntry, object> compute)
        {
            if (string.IsNullOrEmpty(name)) { throw new ArgumentNullException(nameof(name)); }
            Add(new Lambda(name, compute ?? throw new ArgumentNullException(nameof(compute))));
            return this;
        }

        public IEnumerator<IComputedProperty> GetEnumerator() => _properties.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public static explicit operator Dictionary<string, object>(ComputedPropertyCollection properties) => properties.ToDictionary(x => x.Name, x => (object)x);
    }

    internal class ComputedPropertyComparer : IEqualityComparer<IComputedProperty>
    {
        public bool Equals(IComputedProperty x, IComputedProperty y)
        {
            return 
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(IComputedProperty obj) => obj.Name.GetHashCode();
    }
}
