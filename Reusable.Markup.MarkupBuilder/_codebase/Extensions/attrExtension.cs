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
            if (binder.Name == "attr")
            {
                builder.Attributes.Add(binder.Name, (string)args[0]);
                result = builder;
                return true;
            }
            result = null;
            return false;
        }
    }
}