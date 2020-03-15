using Newtonsoft.Json.Linq;

namespace Reusable.Utilities.JsonNet.Visitors
{
    public class TrimPropertyName : JsonVisitor
    {
        protected override JProperty VisitProperty(JProperty property)
        {
            return new JProperty(property.Name.Trim(), Visit(property.Value));
        }
    }
}