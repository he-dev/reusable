using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Reusable.Extensions;

namespace Reusable.Markup
{
    public interface IMarkupElement : ICollection<object>, IFormattable
    {
        string Name { get; }
        IDictionary<string, string> Attributes { get; }
        IMarkupElement Parent { get; set; }
        int Depth { get; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class MarkupElement : IMarkupElement
    {
        private readonly List<object> _content = new List<object>();

        private MarkupElement(string name)
        {
            Name = name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private string DebuggerDisplay => $"<{Name} attribute-count=\"{Attributes?.Count ?? 0}\" children-count=\"{Count}\">";

        public static IMarkupElement Builder => default(IMarkupElement);

        #region IMarkupElement

        public string Name { get; }
        public IDictionary<string, string> Attributes { get; }
        public IMarkupElement Parent { get; set; }
        public int Depth => Parent?.Depth + 1 ?? 0;

        #endregion

        public static IMarkupElement Create(string name, IMarkupElement parent = default(IMarkupElement))
        {
            var element = new MarkupElement(name);
            parent?.Add(element);
            return element;
        }

        #region ICollection<object>

        public int Count => _content.Count;
        public bool IsReadOnly => false;
        public void Add(object item)
        {
            if (item is IMarkupElement markupElement) { markupElement.Parent = this; }
            _content.Add(item);
        }
        public bool Contains(object item) => _content.Contains(item);
        public bool Remove(object item) => _content.Remove(item);
        public void Clear() => _content.Clear();
        void ICollection<object>.CopyTo(object[] array, int arrayIndex) => _content.CopyTo(array, arrayIndex);
        public IEnumerator<object> GetEnumerator() => _content.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return
                formatProvider.GetFormat(typeof(IMarkupElement)) is ICustomFormatter formatter
                    ? formatter.Format(format, this, formatProvider)
                    : base.ToString();
        }
    }
}