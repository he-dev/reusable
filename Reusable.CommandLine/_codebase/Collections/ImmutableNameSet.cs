using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Shelly.Collections
{
    public static class ImmutableNameSet
    {
        public static ImmutableHashSet<string> Create(IEnumerable<string> values) => Create(values.ToArray());

        public static ImmutableHashSet<string> Create(params string[] values) => ImmutableHashSet.Create(StringComparer.OrdinalIgnoreCase, values);
    }
}
