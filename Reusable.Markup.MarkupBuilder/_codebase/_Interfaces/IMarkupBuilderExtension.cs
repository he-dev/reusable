using System.Dynamic;

namespace Reusable
{
    public interface IMarkupBuilderExtension
    {
        bool TryGetMember(MarkupBuilder element, GetMemberBinder binder, out object result);
        bool TryInvokeMember(MarkupBuilder element, InvokeMemberBinder binder, object[] args, out object result);
    }
}