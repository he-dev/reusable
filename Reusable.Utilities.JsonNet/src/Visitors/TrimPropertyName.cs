using Newtonsoft.Json.Linq;
using Reusable.Utilities.JsonNet.Abstractions;

namespace Reusable.Utilities.JsonNet.Visitors;

public class TrimPropertyName : JsonVisitor
{
    protected override JProperty VisitProperty(JProperty property)
    {
        return new JProperty(property.Name.Trim(), Visit(property.Value));
    }
}