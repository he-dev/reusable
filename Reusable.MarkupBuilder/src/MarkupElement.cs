using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.MarkupBuilder
{
    public interface IMarkupElement : ICollection<object>, IFormattable
    {
        //IEqualityComparer<string> Comparer { get; }
        string Name { get; }
        IDictionary<string, string> Attributes { get; }
        IMarkupElement Parent { get; }
        int Depth { get; }
    }
    
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public abstract class MarkupElement : Collection<object>, IMarkupElement
    {
        protected MarkupElement([NotNull] string name, [NotNull] IEqualityComparer<string> comparer)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            //Comparer = comparer ?? throw new ArgumentNullException(nameof(comparer));
            Attributes = new Dictionary<string, string>(comparer);
        }
        
        protected MarkupElement([NotNull] string name, IList<object> content) : base(content)
        {
            Name = name;
            //Comparer = SoftString.Comparer;
            Attributes = new Dictionary<string, string>(SoftString.Comparer);
        }

        private string DebuggerDisplay => $"<{Name} attribute-count=\"{Attributes?.Count ?? 0}\" children-count=\"{Count}\">";

        #region IMarkupElement

        //public IEqualityComparer<string> Comparer { get; }

        public string Name { get; }

        public IDictionary<string, string> Attributes { get; }

        public IMarkupElement Parent { get; private set; }

        public int Depth => Parent?.Depth + 1 ?? 0;

        #endregion

        #region Collection

        public bool IsReadOnly => false;

        protected override void InsertItem(int index, object item)
        {
            SetParent(item);
            base.InsertItem(index, item);
        }

        protected override void SetItem(int index, object item)
        {
            SetParent(item);
            base.SetItem(index, item);
        }

        #endregion

        // Sets this element as the parent of the item being added if it's a markup-element.
        private void SetParent(object item)
        {
            if (item is MarkupElement markupElement)
            {
                markupElement.Parent = this;
            }
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            return
                formatProvider.GetFormat(typeof(ICustomFormatter)) is ICustomFormatter formatter
                    ? formatter.Format(format, this, formatProvider)
                    : base.ToString();
        }

        //public static implicit operator T(MarkupElement<T> element) => element.Name;
    }
}