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
        CaseInsensitiveString Name { get; }

        [NotNull, ItemNotNull]
        IImmutableSet<Type> CustomTypes { get; }

        [CanBeNull]
        ISetting Read([NotNull, ItemNotNull] IEnumerable<CaseInsensitiveString> names);

        void Write([NotNull] ISetting setting);
    }

    [PublicAPI]
    public abstract class Datastore : IDatastore
    {
        private static volatile int _instanceCounter;

        private CaseInsensitiveString _name;

        private Datastore()
        {
            Name = CreateDefaultName(GetType());
        }

        protected Datastore(IEnumerable<Type> supportedTypes) : this()
        {
            CustomTypes = (supportedTypes ?? throw new ArgumentNullException(nameof(supportedTypes))).ToImmutableHashSet();
        }

        public CaseInsensitiveString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public IImmutableSet<Type> CustomTypes { get; }

        public ISetting Read(IEnumerable<CaseInsensitiveString> names)
        {
            try
            {
                return ReadCore(names);
            }
            catch (Exception innerException)
            {
                throw new DatastoreReadException(this, names.Last(), innerException);
            }
        }

        protected abstract ISetting ReadCore(IEnumerable<CaseInsensitiveString> names);

        public void Write(ISetting setting)
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

        protected abstract void WriteCore(ISetting setting);

        protected static string CreateDefaultName(Type datastoreType)
        {
            return $"{datastoreType.Name}{_instanceCounter++}";
        }

        public bool Equals(IDatastore other) => Equals(other?.Name);

        public bool Equals(string other) => Name.Equals(other);
    }

    public class DatastoreReadException : Exception
    {
        public DatastoreReadException(IDatastore datastore, CaseInsensitiveString name, Exception innerException)
        : base($"Could not read '{name.ToString()}' from '{datastore.Name.ToString()}'.", innerException) { }
    }

    public class DatastoreWriteException : Exception
    {
        public DatastoreWriteException(IDatastore datastore, CaseInsensitiveString name, Exception innerException)
            : base($"Could not write '{name.ToString()}' to '{datastore.Name.ToString()}'.", innerException) { }
    }
}