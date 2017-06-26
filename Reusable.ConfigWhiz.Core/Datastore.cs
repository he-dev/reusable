using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Paths;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz
{
    public interface IDatastore : IEquatable<IDatastore>, IEquatable<string>
    {
        string Name { get; }
        IImmutableSet<Type> SupportedTypes { get; }
        ICollection<ISetting> Read(Identifier identifier);
        int Write(IGrouping<Identifier, ISetting> settings);
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

        public ICollection<ISetting> Read(Identifier identifier)
        {
            try
            {
                return ReadCore(identifier);
            }
            catch (Exception innerException)
            {
                throw new DatastoreReadException(this, identifier, innerException);
            }
        }

        protected abstract ICollection<ISetting> ReadCore(Identifier identifier);

        public int Write(IGrouping<Identifier, ISetting> settings)
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

        protected abstract int WriteCore(IGrouping<Identifier, ISetting> settings);

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
        public DatastoreReadException(IDatastore datastore, Identifier identifier, Exception innerException)
        : base($"Could not read '{identifier}' from '{datastore.Name}'.", innerException) { }
    }

    public class DatastoreWriteException : Exception
    {
        public DatastoreWriteException(IDatastore datastore, Identifier identifier, Exception innerException)
            : base($"Could not write '{identifier}' to '{datastore.Name}'.", innerException) { }
    }
}