using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Datastores
{
    public class Memory : Datastore, IEnumerable<IEntity>
    {
        public Memory() : this(CreateDefaultName<Memory>()) { }

        public Memory(string name)
            : base(name, new[] { typeof(string) })
        { }

        protected Memory(string name, IEnumerable<Type> supportedTypes)
            : base(name, supportedTypes)
        { }

        protected override ICollection<IEntity> ReadCore(IIdentifier id)
        {
            var name = id.ToString();
            var settings =
                (from x in Data
                 where x.Id.ToString().StartsWith(name, StringComparison.OrdinalIgnoreCase)
                 select x).ToList();
            return settings;
        }

        protected override int WriteCore(IGrouping<IIdentifier, IEntity> settings)
        {
            var name = settings.Key.ToString(); //($".{IdentifierLength}" IdentifierFormat.FullWeak, IdentifierFormatter.Instance);
            var obsoleteSettings =
                (from x in Data
                 where x.Id.ToString().StartsWith(name, StringComparison.OrdinalIgnoreCase)
                 select x).ToList();
            obsoleteSettings.ForEach(x => Data.Remove(x));

            foreach (var setting in settings) Add(setting);

            return obsoleteSettings.Count;
        }

        public List<IEntity> Data { [DebuggerStepThrough] get; [DebuggerStepThrough] set; } = new List<IEntity>();

        #region IEnumerable

        public void Add(IEntity entity) => Data.Add(entity);

        public void Add(string name, object value) => Data.Add(new Entity
        {
            Id = Identifier.Parse(name),
            Value = value
        });

        public Memory AddRange(IEnumerable<IEntity> settings)
        {
            foreach (var setting in settings) Add(setting);
            return this;
        }

        public IEnumerator<IEntity> GetEnumerator() => Data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}