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
        IImmutableSet<Type> CustomTypes { get; }

        [CanBeNull]
        IEntity Read([NotNull, ItemNotNull] IEnumerable<CaseInsensitiveString> names);

        void Write([NotNull] IEntity setting);
    }

    public abstract class Datastore : IDatastore
    {
        private static volatile int _instanceCounter;

        protected Datastore(string name, IEnumerable<Type> supportedTypes)
        {
            Name = name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
            CustomTypes = (supportedTypes ?? throw new ArgumentNullException(nameof(supportedTypes))).ToImmutableHashSet();
        }

        public string Name { get; }

        public IImmutableSet<Type> CustomTypes { get; }

        public IEntity Read(IEnumerable<CaseInsensitiveString> names)
        {
            try
            {
                return ReadCore(names);
            }
            catch (Exception innerException)
            {
                throw new DatastoreReadException(this, names, innerException);
            }
        }

        protected abstract IEntity ReadCore(IEnumerable<CaseInsensitiveString> names);

        public void Write(IEntity setting)
        {
            try
            {
                WriteCore(setting);
            }
            catch (Exception innerException)
            {
                throw new DatastoreWriteException(this, setting.Name, innerException);
            }
        }

        protected abstract void WriteCore(IEntity setting);

        protected static string CreateDefaultName<T>()
        {
            return $"{typeof(T).Name}{_instanceCounter++}";
        }

        public bool Equals(IDatastore other) => Equals(other?.Name); 

        public bool Equals(string other) => Name.Equals(other, StringComparison.OrdinalIgnoreCase);
    }

    public class DatastoreReadException : Exception
    {
        public DatastoreReadException(IDatastore datastore, IEnumerable<CaseInsensitiveString> names, Exception innerException)
        : base($"Could not read {string.Join(", ", names.ToJson())} from '{datastore.Name}'.", innerException) { }
    }

    public class DatastoreWriteException : Exception
    {
        public DatastoreWriteException(IDatastore datastore, CaseInsensitiveString name, Exception innerException)
            : base($"Could not write '{name.ToString()}' to '{datastore.Name}'.", innerException) { }
    }
}