using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Markup
{
    public class MarkupBuilderExtensionCollection : IEnumerable<IMarkupBuilderExtension>
    {
        private readonly HashSet<IMarkupBuilderExtension> _extensions = new HashSet<IMarkupBuilderExtension>(new TypeComparer());

        public MarkupBuilderExtensionCollection Add(IMarkupBuilderExtension extension)
        {
            if (!_extensions.Add(extension)) throw new ArgumentException($"Extension \"{extension.GetType().Name}\" is already added.");
            return this;
        }

        public MarkupBuilderExtensionCollection Add<T>() where T : IMarkupBuilderExtension, new() => Add(new T());

        public IEnumerator<IMarkupBuilderExtension> GetEnumerator() => _extensions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    internal class TypeComparer : IEqualityComparer<IMarkupBuilderExtension>
    {
        public bool Equals(IMarkupBuilderExtension x, IMarkupBuilderExtension y) => x.GetType() == y.GetType();

        public int GetHashCode(IMarkupBuilderExtension obj) => obj.GetType().GetHashCode();
    }
}
