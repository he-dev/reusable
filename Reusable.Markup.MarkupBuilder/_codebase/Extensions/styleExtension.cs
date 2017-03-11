using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Markup.Extensions
{
    public class styleExtension : IMarkupBuilderExtension
    {
        private IDictionary<string, string> _styles;

        public styleExtension(IDictionary<string, string> styles) => _styles = new Dictionary<string, string>(styles);

        public IDictionary<string, string> Styles
        {
            get => _styles;
            set => _styles = value ?? throw new ArgumentNullException("Styles");
        }

        public bool TryGetMember(MarkupBuilder builder, GetMemberBinder binder, out object result)
        {
            result = null;
            return false;
        }

        public bool TryInvokeMember(MarkupBuilder builder, InvokeMemberBinder binder, object[] args, out object result)
        {
            switch (binder.Name)
            {
                case "style":
                    if (args == null || args.Length != 1) throw new ArgumentException(paramName: nameof(args), message: "style must have exactly one argument.");

                    var styleName = (string)args[0];
                    if (_styles.TryGetValue(styleName, out string style))
                    {
                        builder.Attributes.Add(binder.Name, style);
                        result = builder;
                        return true;
                    }
                    else
                    {
                        throw new ArgumentException($"Style '{styleName}' not found.");
                    }

                default:
                    result = null;
                    return false;
            }
        }
    }
}
