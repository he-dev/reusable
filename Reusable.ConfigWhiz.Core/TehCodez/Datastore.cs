using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface IDatastore : IEquatable<IDatastore>, IEquatable<string>
    {
        [NotNull]
        string Name { get; }

        [NotNull, ItemNotNull]
        IImmutableSet<Type> SupportedTypes { get; }

        [NotNull, ItemNotNull]
        ICollection<IEntity> Read([NotNull] IIdentifier id);

        int Write([NotNull, ItemNotNull] IGrouping<IIdentifier, IEntity> settings);
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

        public ICollection<IEntity> Read(IIdentifier id)
        {
            try
            {
                return ReadCore(id);
            }
            catch (Exception innerException)
            {
                throw new DatastoreReadException(this, id, innerException);
            }
        }

        protected abstract ICollection<IEntity> ReadCore(IIdentifier id);

        public int Write(IGrouping<IIdentifier, IEntity> settings)
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

        protected abstract int WriteCore(IGrouping<IIdentifier, IEntity> settings);

        protected static string CreateDefaultName<T>()
        {
            return $"{typeof(T).Name}{_instanceCounter++}";
        }

        public bool Equals(IDatastore other) => Equals(other?.Name); 

        public bool Equals(string other) => Name.Equals(other, StringComparison.OrdinalIgnoreCase);
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
        public DatastoreReadException(IDatastore datastore, IIdentifier id, Exception innerException)
        : base($"Could not read '{id}' from '{datastore.Name}'.", innerException) { }
    }

    public class DatastoreWriteException : Exception
    {
        public DatastoreWriteException(IDatastore datastore, IIdentifier id, Exception innerException)
            : base($"Could not write '{id}' to '{datastore.Name}'.", innerException) { }
    }
}