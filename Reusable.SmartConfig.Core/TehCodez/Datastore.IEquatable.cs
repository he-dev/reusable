using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public abstract partial class SettingDataStore
    {
        public override int GetHashCode() => AutoEquality<ISettingDataStore>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => Equals(obj as ISettingDataStore);

        public bool Equals(ISettingDataStore other) => AutoEquality<ISettingDataStore>.Comparer.Equals(this, other);

        //public bool Equals(string other) => Name.Equals(other);
    }
}