using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.ConfigWhiz.Extensions;
using Reusable.Data.Annotations;
using Reusable.Extensions;

namespace Reusable.ConfigWhiz.Data
{
    public class SettingContainer : IEquatable<IIdentifier>, IEnumerable<Setting>
    {
        [NotNull]
        private readonly object _instance;

        [NotNull, ItemNotNull]
        private readonly IEnumerable<Setting> _settings;

        private SettingContainer(IIdentifier id, object instance)
        {
            Id = id;
            _instance = instance;

            _settings =
                (from property in instance.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                 where property.GetCustomAttribute<IgnoreAttribute>().IsNull()
                 select new Setting(Identifier.From(id, property.GetCustomNameOrDefault()), instance, property)).ToList();
        }

        [NotNull]
        public IIdentifier Id { get; }

        public static SettingContainer Create<TContainer>(IIdentifier identifier) where TContainer : class, new()
        {
            return new SettingContainer(identifier, new TContainer());
        }                

        public T As<T>() where T : class => _instance as T;

        #region IEquatable<Container>

        public bool Equals(IIdentifier other)
        {
            if (ReferenceEquals(null, other)) return false;
            return ReferenceEquals(Id, other) || Id.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return 
                (obj is Identifier identifier && Equals(identifier)) || 
                (obj is SettingContainer container && Equals(container.Id));
        }

        public override int GetHashCode() => Id.GetHashCode();

        #endregion

        public IEnumerator<Setting> GetEnumerator() => _settings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}