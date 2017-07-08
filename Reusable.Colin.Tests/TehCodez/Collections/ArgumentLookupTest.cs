using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine.Collections;
using Reusable.CommandLine.Data;
using Reusable.TestTools.UnitTesting.Infrastructure.Collections;

namespace Reusable.CommandLine.Tests.Collections
{
    [TestClass]
    public class ArgumentLookupTest : LookupTest<IImmutableNameSet, string>
    {
        protected override ILookup<IImmutableNameSet, string> GetEmptyLookup()
        {
            return new ArgumentLookup();
        }

        protected override ILookup<IImmutableNameSet, string> GetNonEmptyLookup()
        {
            return new ArgumentLookup
            {
                new ArgumentGrouping(ImmutableNameSet.Create("arg1")) { "true" },
                new ArgumentGrouping(ImmutableNameSet.Create("arg2")),
                new ArgumentGrouping(ImmutableNameSet.Create("arg3")) { "1", "2" }
            };
        }

        protected override IEnumerable<(IImmutableNameSet Key, IEnumerable<string> Elements)> GetExistingKeys()
        {
            yield return (ImmutableNameSet.Create("arg1"), new[] { "true" });
            yield return (ImmutableNameSet.Create("arg2"), new string[0]);
            yield return (ImmutableNameSet.Create("arg3"), new[] { "1", "2" });
        }

        protected override IEnumerable<(IImmutableNameSet Key, IEnumerable<string> Elements)> GetNonExistingKeys()
        {
            yield return (ImmutableNameSet.Create("arg4"), Enumerable.Empty<string>());
            yield return (ImmutableNameSet.Create("arg5"), Enumerable.Empty<string>());
        }
    }
}
