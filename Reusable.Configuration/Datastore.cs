using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.ConfigWhiz.Data;

namespace Reusable.ConfigWhiz
{
    public interface IDatastore
    {
        IImmutableSet<Type> SupportedTypes { get; }
        Result<IEnumerable<ISetting>> Read(SettingPath path);
        Result Write(IGrouping<SettingPath, ISetting> settings);
    }

    public abstract class Datastore : IDatastore
    {
        protected Datastore(IEnumerable<Type> supportedTypes)
        {
            SupportedTypes = supportedTypes.ToImmutableHashSet();
        }

        public IImmutableSet<Type> SupportedTypes { get; }

        public abstract Result<IEnumerable<ISetting>> Read(SettingPath path);

        public abstract Result Write(IGrouping<SettingPath, ISetting> settings);
    }
}