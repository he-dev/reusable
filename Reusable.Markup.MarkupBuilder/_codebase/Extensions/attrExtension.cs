using System.Dynamic;
using Reusable.Fuse;

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
            if (binder.Name == "attr")
            {
                args.Validate(nameof(args)).IsNotNull().Then(x => x.Length, nameof(args.Length)).IsEqual(2);

                builder.Attributes.Add((string)args[0], (string)args[1]);
                result = builder;
                return true;
            }
            result = null;
            return false;
        }
    }
}