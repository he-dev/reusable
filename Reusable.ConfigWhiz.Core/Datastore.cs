using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Reusable.ConfigWhiz.Data;

namespace Reusable.ConfigWhiz
{
    public interface IDatastore
    {
        string Handle { get; }
        IImmutableSet<Type> SupportedTypes { get; }
        Result<IEnumerable<ISetting>> Read(SettingPath settingPath);
        Result Write(IGrouping<SettingPath, ISetting> settings);
    }

    public abstract class Datastore : IDatastore
    {
        protected Datastore(object handle, IEnumerable<Type> supportedTypes)
        {
            Handle = handle == DatastoreHandle.Default ? GetType().Name : handle.ToString();
            SupportedTypes = supportedTypes.ToImmutableHashSet();
        }

        public string Handle { get; }

        public IImmutableSet<Type> SupportedTypes { get; }

        public abstract Result<IEnumerable<ISetting>> Read(SettingPath settingPath);

        public abstract Result Write(IGrouping<SettingPath, ISetting> settings);
    }

    public class DatastoreHandle
    {
        private DatastoreHandle() { }
        public static readonly DatastoreHandle Default = new DatastoreHandle();
    }

    public class DatasourceComparer : IEqualityComparer<IDatastore>
    {
        public bool Equals(IDatastore x, IDatastore y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.Handle == y.Handle;
        }

        public int GetHashCode(IDatastore obj)
        {
            return obj.GetHashCode();
        }
    }
}