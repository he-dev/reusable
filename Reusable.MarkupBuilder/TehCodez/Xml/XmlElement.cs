using System;
using JetBrains.Annotations;

namespace Reusable.MarkupBuilder.Xml
{
    
    // Marker interface for XmlElement extensions.
    public interface IXmlElement : IMarkupElement
    {
    }
    
    public class XmlElement : MarkupElement, IXmlElement
    {
        public XmlElement([NotNull] string name) : base(name, StringComparer.Ordinal)
        {
        }

        public static XmlElement Builder => default;

        public static XmlElement Create(string name) => new XmlElement(name);
    }
}