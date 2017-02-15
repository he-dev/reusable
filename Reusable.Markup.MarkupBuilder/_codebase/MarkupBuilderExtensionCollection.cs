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
        private readonly List<IMarkupBuilderExtension> _extensions = new List<IMarkupBuilderExtension>();

        public MarkupBuilderExtensionCollection Add<T>() where T : IMarkupBuilderExtension, new()
        {
            if (_extensions.OfType<T>().Any())
            {
                throw new ArgumentException($"Extension \"{typeof(T).Name}\" is already added.");
            }

            _extensions.Add(new T());
            return this;
        }

        public T Get<T>() where T : IMarkupBuilderExtension
        {
            return _extensions.OfType<T>().SingleOrDefault();
        }

        public IEnumerator<IMarkupBuilderExtension> GetEnumerator()
        {
            return _extensions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
