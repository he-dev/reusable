using System.Collections.Generic;
using Reusable.Essentials.Collections;
using Reusable.Essentials.Reflection;

namespace Reusable.Quickey
{
    public class SelectorContext
    {
        public MemberMetadata Metadata { get; set; } = default!;

        public ISet<ISelectorTokenFactoryParameter> TokenParameters { get; } = new HashSet<ISelectorTokenFactoryParameter>();

        private static readonly IEqualityComparer<ISelectorTokenFactoryParameter> TokenParameterComparer = EqualityComparer.Create<ISelectorTokenFactoryParameter>
        (
            getHashCode: (obj) => 0,
            equals: (left, right) => left.GetType() == right.GetType()
        );
    }

    public static class HashSetExtensions
    {
        public static void Add<T>(this ISet<T> hashSet, IEnumerable<T> values) => hashSet.UnionWith(values);
    }
}