using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public abstract partial class Datastore
    {
        public override int GetHashCode() => AutoEquality<IDatastore>.Comparer.GetHashCode(this);

        public override bool Equals(object obj) => Equals(obj as IDatastore);

        public bool Equals(IDatastore other) => AutoEquality<IDatastore>.Comparer.Equals(this, other);

        //public bool Equals(string other) => Name.Equals(other);
    }
}