using System;

namespace Reusable.SmartConfig.Data
{
    public class EntityProperty
    {
        private readonly string _name;
        private EntityProperty(string name) { _name = name; }
        public static readonly EntityProperty Name = new EntityProperty(nameof(Name));
        public static readonly EntityProperty Value = new EntityProperty(nameof(Value));
        //public static readonly IEnumerable<SettingProperty> Default = new[] { Name, Value };
        public static bool Exists(string name) =>
            name.Equals(nameof(Name), StringComparison.OrdinalIgnoreCase) ||
            name.Equals(nameof(Value), StringComparison.OrdinalIgnoreCase);
        public override string ToString() => _name;
        public static implicit operator string(EntityProperty entityProperty) => entityProperty._name;
    }
}