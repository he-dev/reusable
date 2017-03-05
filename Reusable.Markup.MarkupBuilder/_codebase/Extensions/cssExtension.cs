using System;
using System.Dynamic;

namespace Reusable.Markup.Extensions
{
    // ReSharper disable once InconsistentNaming
    public class cssExtension : IMarkupBuilderExtension
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
                case "css":
                    if (args == null || args.Length != 1) throw new ArgumentException(paramName: nameof(args), message: "css must have exactly one argument.");
                    builder.Attributes.Add("style", (string)args[0]);
                    result = builder;
                    return true;

                default:
                    result = null;
                    return false;
            }
        }
    }
}