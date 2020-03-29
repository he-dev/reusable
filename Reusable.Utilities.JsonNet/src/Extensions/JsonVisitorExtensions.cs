using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Reusable.Utilities.JsonNet.Abstractions;

namespace Reusable.Utilities.JsonNet.Extensions
{
    public static class JsonVisitorExtensions
    {
        public static JToken Visit(this IEnumerable<IJsonVisitor> visitors, JToken token)
        {
            return visitors.Aggregate(token, (current, visitor) => visitor.Visit(current));
        }
    }
}