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
        void AddRange(IEnumerable<object> items);
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class MarkupElement : IMarkupElement
    {
        private readonly List<object> _content = new List<object>();

        public MarkupElement(string name, IEnumerable<object> content)
        {
            Name = name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
            AddRange(content ?? throw new ArgumentNullException(nameof(content)));
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public MarkupElement(string name, IEnumerable<Func<IMarkupElement, object>> content)
        {
            Name = name.NullIfEmpty() ?? throw new ArgumentNullException(nameof(name));
            foreach (var item in content ?? throw new ArgumentNullException(nameof(content))) item(this);
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        private string DebuggerDisplay => $"<{Name} attribute-count=\"{Attributes?.Count ?? 0}\" children-count=\"{Count}\">";

        #region IMarkupElement

        public static IMarkupElement Builder => default(IMarkupElement);
        public string Name { get; }
        public IDictionary<string, string> Attributes { get; }
        public IMarkupElement Parent { get; set; }

        public int Depth => Parent?.Depth + 1 ?? 0;

        #endregion

        #region ICollection<object>

        public int Count => _content.Count;
        public bool IsReadOnly => false;
        public void Add(object item)
        {
            switch (item)
            {
                case IMarkupElement e: e.Parent = this; break;
            }
            _content.Add(item);
        }
        public bool Contains(object item) => _content.Contains(item);
        public bool Remove(object item) => _content.Remove(item);
        public void Clear() => _content.Clear();
        void ICollection<object>.CopyTo(object[] array, int arrayIndex) => _content.CopyTo(array, arrayIndex);
        public IEnumerator<object> GetEnumerator() => _content.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion       

        public void AddRange(IEnumerable<object> items)
        {
            foreach (var item in items ?? throw new ArgumentNullException(nameof(items))) Add(item);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return 
                (formatProvider.GetFormat(typeof(MarkupFormatter)) is ICustomFormatter formatter) 
                    ? formatter.Format(format, this, formatProvider) 
                    : base.ToString();
        }
    }
}