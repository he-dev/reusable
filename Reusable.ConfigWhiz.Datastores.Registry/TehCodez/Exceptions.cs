using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.SmartConfig.Datastores.TehCodez
{
    public class InvalidTypeException : Exception
    {
        internal InvalidTypeException(Type invalidType, IEnumerable<Type> validTypes)
        {
            InvalidType = invalidType;
            ValidTypes = validTypes;
        }

        public Type InvalidType { get; }

        public IEnumerable<Type> ValidTypes { get; }

        public override string Message => $"Invalid type \"{InvalidType.Name}\". Valid types are [{string.Join(", ", ValidTypes.Select(x => x.Name))}].";

        //public override string ToString() => this.ToJson();
    }

    public class SubKeyException : Exception
    {
        internal SubKeyException(string baseKeyName, string baseKeySubName, string subKeyName)
        {
            BaseKeyName = baseKeyName;
            BaseKeySubName = baseKeySubName;
            SubKeyName = subKeyName;
        }

        public string BaseKeyName { get; }
        public string BaseKeySubName { get; set; }
        public string SubKeyName { get; }

        public override string Message => $"Could not open or create \"{BaseKeyName}\\{BaseKeySubName}\\{SubKeyName}\".";
    }
}
