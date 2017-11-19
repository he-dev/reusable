using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface IDatastore : IEquatable<IDatastore>
    {
        [AutoEqualityProperty]
        [NotNull]
        SoftString Name { get; }

        [NotNull, ItemNotNull]
        ISet<Type> CustomTypes { get; }

        [CanBeNull]
        ISetting Read([NotNull, ItemNotNull] IEnumerable<SoftString> names);

        void Write([NotNull] ISetting setting);
    }

    [PublicAPI]
    public abstract partial class Datastore : IDatastore
    {
        private static volatile int _instanceCounter;

        private SoftString _name;

        private Datastore()
        {
            Name = CreateDefaultName(GetType());
        }

        protected Datastore(IEnumerable<Type> supportedTypes) : this()
        {
            CustomTypes = new HashSet<Type>(supportedTypes ?? throw new ArgumentNullException(nameof(supportedTypes)));
        }

        public SoftString Name
        {
            get => _name;
            set => _name = value ?? throw new ArgumentNullException(nameof(Name));
        }

        public ISet<Type> CustomTypes { get; }

        public ISetting Read(IEnumerable<SoftString> names)
        {
            try
            {
                return ReadCore(names);
            }
            catch (Exception innerException)
            {
                throw ("SettingReadException", $"Cannot read {names.Last().ToString().QuoteWith("'")} from {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        protected abstract ISetting ReadCore(IEnumerable<SoftString> names);

        public void Write(ISetting setting)
        {
            try
            {
                WriteCore(setting);
            }
            catch (Exception innerException)
            {
                throw ("SettingWriteException", $"Cannot write {setting.Name.ToString().QuoteWith("'")} to {Name.ToString().QuoteWith("'")}.", innerException).ToDynamicException();
            }
        }

        protected abstract void WriteCore(ISetting setting);

        protected static string CreateDefaultName(Type datastoreType)
        {
            return $"{datastoreType.Name}{_instanceCounter++}";
        }
    }
}