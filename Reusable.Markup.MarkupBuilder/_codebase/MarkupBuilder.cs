using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Reusable.Markup
{
    public interface IElement : IEnumerable<object>
    {
        string Name { get; }
        IDictionary<string, string> Attributes { get; }
        IList<object> Content { get; }
        IElement Parent { get; }
    }

    public class MarkupBuilder : DynamicObject, IElement
    {
        private MarkupBuilder(MarkupBuilder markupBuilder, string name)
        {
            Extensions = markupBuilder.Extensions;
            Renderer = markupBuilder.Renderer;
            Name = name;
        }

        public MarkupBuilder(IMarkupRenderer renderer)
        {
            Renderer = renderer;
        }

        // The first builder has no tag and thus is not a real element.
        private bool IsContainer => !string.IsNullOrEmpty(Name);

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

        #region IElement

        public string Name { get; }

        public IDictionary<string, string> Attributes { get; } = new Dictionary<string, string>();

        public IList<object> Content { get; } = new List<object>();

        public IElement Parent { get; private set; }

        public IEnumerator<object> GetEnumerator() => Content.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region DynamicObject

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

        //public override bool TryGetMember(GetMemberBinder binder, out object result)
        //{
        //    foreach (var extension in Extensions)
        //    {
        //        if (extension.TryGetMember(this, binder, out result))
        //        {
        //            return true;
        //        }
        //    }
        //    result = CreateElement(binder.Name);
        //    return true;
        //}

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            foreach (var extension in Extensions)
            {
                if (extension.TryInvokeMember(this, binder, args, out result))
                {
                    return true;
                }
            }

            var element = CreateElement(binder.Name);

            foreach (var arg in args.Where(x => x != null))
            {
                element.Add(arg);
            }

            result = element;
            return true;
        }

        #endregion

        private MarkupBuilder CreateElement(string name)
        {
            var element = new MarkupBuilder(this, name);
            if (IsContainer)
            {
                Add(element);
            }
            return element;
        }

        public void Add(object obj)
        {
            if (obj == null)
            {
                return;
            }


            var builder = obj as MarkupBuilder;
            if (builder != null)
            {
                Content.Add(obj);
                if (IsContainer)
                {
                    builder.Parent = this;
                }
                return;
            }
            else
            {
                var objects = obj as IEnumerable<object>;
                if (objects != null)
                {
                    AddRange(objects);
                    return;
                }
            }

            Content.Add(obj);
        }

        private void AddRange(IEnumerable<object> content)
        {
            foreach (var item in content)
            {
                Add(item);
            }
        }

        public override string ToString()
        {
            return Renderer?.Render(this) ?? base.ToString();
        }
    }
}
