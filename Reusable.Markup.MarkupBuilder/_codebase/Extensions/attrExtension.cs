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
            if (binder.Name == "attr")
            {
                if (args == null) throw new ArgumentNullException(nameof(args));
                if (args.Length != 2) throw new ArgumentOutOfRangeException(nameof(args), "There must be exactly two arguments.");
                //args.Validate(nameof(args)).IsNotNull().Then(x => x.Length, nameof(args.Length)).IsEqual(2);

                builder.Attributes.Add((string)args[0], (string)args[1]);
                result = builder;
                return true;
            }

            result = null;
            return false;
        }
    }
}