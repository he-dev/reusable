using System;
using System.Dynamic;

namespace Reusable.Markup.Extensions
{
    // ReSharper disable once InconsistentNaming
    public class attrExtension : IMarkupBuilderExtension
    {
        public bool TryGetMember(MarkupBuilder builder, GetMemberBinder binder, out object result)
        {
            result = null;
            return false;
        }

        public bool TryInvokeMember(MarkupBuilder builder, InvokeMemberBinder binder, object[] args, out object result)
        {
            switch (binder.Name)
            {
                case "attr":
                    if (args == null || args.Length != 2) throw new ArgumentException(nameof(args), "attr must have exactly two arguments: attribute name and attribute value.");
                    builder.Attributes.Add((string)args[0], (string)args[1]);
                    result = builder;
                    return true;

                default:
                    result = null;
                    return false;
            }
        }
    }
}