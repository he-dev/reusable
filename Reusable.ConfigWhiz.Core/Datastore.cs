using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.ConfigWhiz.Data;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz
{
    public interface IDatastore
    {
        string Name { get; }
        IImmutableSet<Type> SupportedTypes { get; }
        ICollection<ISetting> Read(SettingPath settingPath);
        int Write(IGrouping<SettingPath, ISetting> settings);
    }

    public abstract class Datastore : IDatastore
    {
        private static volatile int _instanceCounter;

        protected Datastore(string name, IEnumerable<Type> supportedTypes)
        {
            Name = name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
            SupportedTypes = (supportedTypes ?? throw new ArgumentNullException(nameof(supportedTypes))).ToImmutableHashSet();
        }

        public string Name { get; }

        public IImmutableSet<Type> SupportedTypes { get; }

        public abstract ICollection<ISetting> Read(SettingPath settingPath);

        public abstract int Write(IGrouping<SettingPath, ISetting> settings);

        protected static string CreateDefaultName<T>()
        {
            return $"{typeof(T).Name}{_instanceCounter++}";
        }
    }

    public class DatastoreComparer : IEqualityComparer<IDatastore>
    {
        public bool Equals(IDatastore x, IDatastore y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(IDatastore obj)
        {
            return obj.GetHashCode();
        }
    }
}