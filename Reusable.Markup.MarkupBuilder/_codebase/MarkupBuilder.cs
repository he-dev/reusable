using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.Markup
{
    public class MarkupBuilder : DynamicObject, IEnumerable<object>
    {
        private readonly List<object> _content = new List<object>();

        private MarkupBuilder(MarkupBuilder markupBuilder, string tag)
        {
            Extensions = markupBuilder.Extensions;
            Renderer = markupBuilder.Renderer;
            Tag = tag;
        }

        public MarkupBuilder(IMarkupRenderer renderer)
        {
            Renderer = renderer;
        }

        public string Tag { get; }

        // The first builder has no tag and thus is not a real element.
        public bool IsElement => !string.IsNullOrEmpty(Tag);

        public IDictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

        public MarkupBuilder Parent { get; private set; }

        public MarkupBuilderExtensionCollection Extensions { get; } = new MarkupBuilderExtensionCollection();

        internal int Depth
        {
            get
            {
                var depth = 0;
                var parent = Parent;
                while (parent != null)
                {
                    depth++;
                    parent = parent.Parent;
                }
                return depth;
            }
        }

        // Using a renderer + ToString allows us to see the markup in the debug.
        public IMarkupRenderer Renderer { get; }

        public MarkupBuilder Create(string tag)
        {
            var child = new MarkupBuilder(this, tag);
            if (IsElement)
            {
                Add(child);
            }
            return child;
        }       

        public MarkupBuilder Add(object content)
        {
            if (content == null) { return this; }
            _content.Add(content);
            var builder = content as MarkupBuilder;
            if (builder != null) { builder.Parent = this; }
            return this;
        }

        public MarkupBuilder AddRange(IEnumerable<object> content)
        {
            foreach (var item in content) { Add(item); }
            return this;
        }

        public MarkupBuilder AddRange(params object[] content)
        {
            return AddRange((IEnumerable<object>)content);
        }

        public IEnumerator<object> GetEnumerator() => _content.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        // --- DynamicObject members

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            if (binder.ReturnType == typeof(string))
            {
                result = ToString();
                return true;
            }
            result = null;
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            foreach (var extension in Extensions)
            {
                if (extension.TryGetMember(this, binder, out result))
                {
                    return true;
                }
            }
            result = Create(binder.Name);
            return true;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            foreach (var extension in Extensions)
            {
                if (extension.TryInvokeMember(this, binder, args, out result))
                {
                    return true;
                }
            }

            var arg0 = args.FirstOrDefault();

            var isContentEnumerable =
                arg0 != null &&
                arg0.GetType().IsEnumerable() &&
                arg0.GetType() != typeof(string) &&
                arg0.GetType() != typeof(MarkupBuilder);

            var content = isContentEnumerable ? (IEnumerable<object>)arg0 : args;
            result = Create(binder.Name).AddRange(content);
            return true;
        }

        public override string ToString()
        {
            return Renderer?.Render(this) ?? base.ToString();
        }
    }
}
