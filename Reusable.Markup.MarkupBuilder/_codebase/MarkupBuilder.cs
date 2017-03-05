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
        public MarkupBuilder(IMarkupRenderer renderer)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        private MarkupBuilder(IMarkupRenderer renderer, MarkupBuilderExtensionCollection extensions, string name, IEnumerable<object> content)
        {
            Renderer = renderer;
            Extensions = extensions;
            Name = name;
            Add(content);
        }

        public MarkupBuilderExtensionCollection Extensions { get; } = new MarkupBuilderExtensionCollection();

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
            switch (Type.GetTypeCode(binder.ReturnType))
            {
                case TypeCode.String:
                    result = ToString();
                    return true;

                default:
                    result = null;
                    return false;
            }
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
            foreach (var extension in Extensions) if (extension.TryInvokeMember(this, binder, args, out result)) return true;
            result = Add(new MarkupBuilder(Renderer, Extensions, binder.Name, args));
            return true;
        }

        #endregion

        public object Add(object obj)
        {
            switch (obj)
            {
                case MarkupBuilder builder:
                    if (IsElement(builder))
                    {
                        Content.Add(obj);
                        builder.Parent = this;
                    }
                    break;

                case IEnumerable<object> collection:
                    foreach (var item in collection.Where(x => x != null)) Add(item); ;
                    break;

                case null:
                    // Do nothing.
                    break;

                default:
                    // Anything else just add.
                    Content.Add(obj);
                    break;
            }

            return obj;

            // The first builder has no tag and thus is not a real element.
            bool IsElement(MarkupBuilder builder) => !string.IsNullOrEmpty(Name);
        }

        public override string ToString() => Renderer?.Render(this) ?? base.ToString();
    }
}
