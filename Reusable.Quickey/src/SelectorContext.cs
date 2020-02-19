using System.Collections.Generic;
using System.Reflection;
using Reusable.Collections;

namespace Reusable.Quickey
{
    public class SelectorContext
    {
        public MemberInfo Member { get; set; } = default!;

        public SelectorTokenParameterCollection TokenParameters { get; set; } = new SelectorTokenParameterCollection();
    }

    public class SelectorTokenParameterCollection : HashSet<ISelectorTokenFactoryParameter>
    {
        public SelectorTokenParameterCollection(params ISelectorTokenFactoryParameter[] parameters) : base(parameters, EqualityComparer.Create<ISelectorTokenFactoryParameter>
        (
            getHashCode: (obj) => 0,
            equals: (left, right) => left.GetType() == right.GetType()
        )) { }
    }
}