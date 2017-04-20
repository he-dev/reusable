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

        public ICollection<ISetting> Read(SettingPath settingPath)
        {
            try
            {
                return ReadCore(settingPath);
            }
            catch (Exception innerException)
            {
                throw new DatastoreReadException(this, settingPath, innerException);
            }
        }

        protected abstract ICollection<ISetting> ReadCore(SettingPath settingPath);

        public int Write(IGrouping<SettingPath, ISetting> settings)
        {
            try
            {
                return WriteCore(settings);
            }
            catch (Exception innerException)
            {
                throw new DatastoreWriteException(this, settings.Key, innerException);
            }
        }

        protected abstract int WriteCore(IGrouping<SettingPath, ISetting> settings);

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

    public class DatastoreReadException : Exception
    {
        public DatastoreReadException(IDatastore datastore, SettingPath path, Exception innerException)
        : base($"Could not read '{path.ToFullWeakString()}' from '{datastore.Name}'.", innerException) { }
    }

    public class DatastoreWriteException : Exception
    {
        public DatastoreWriteException(IDatastore datastore, SettingPath path, Exception innerException)
            : base($"Could not write '{path.ToFullWeakString()}' to '{datastore.Name}'.", innerException) { }
    }
}