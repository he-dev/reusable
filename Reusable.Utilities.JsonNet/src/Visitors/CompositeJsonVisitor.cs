using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Reusable.Utilities.JsonNet.Abstractions;

namespace Reusable.Utilities.JsonNet.Visitors
{
    /// <summary>
    /// This is an immutable visitor that executes specified visitor in the order they were added.
    /// </summary>
    public class CompositeJsonVisitor : JsonVisitor, IEnumerable<IJsonVisitor>
    {
        private readonly IList<IJsonVisitor> _visitors = new List<IJsonVisitor>();

        //public CompositeJsonVisitor(IEnumerable<IJsonVisitor> visitors) => _visitors = visitors.ToImmutableList();

        //public static CompositeJsonVisitor Empty => new CompositeJsonVisitor(ImmutableList<IJsonVisitor>.Empty);

        public void Add(IJsonVisitor visitor) => _visitors.Add(visitor);

        public override JToken Visit(JToken token) => this.Aggregate(token, (current, visitor) => visitor.Visit(current));

        public IEnumerator<IJsonVisitor> GetEnumerator() => _visitors.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_visitors).GetEnumerator();
    }
}