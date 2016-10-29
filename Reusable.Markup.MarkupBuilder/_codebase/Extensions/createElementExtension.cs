using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Reusable.Markup.Extensions
{
    // ReSharper disable once InconsistentNaming
    public class createElementExtension : IMarkupBuilderExtension
    {
        public IDictionary<string, string> Styles { get; set; }

        public bool TryGetMember(MarkupBuilder builder, GetMemberBinder binder, out object result)
        {
            result = null;
            return false;
        }

        public bool TryInvokeMember(MarkupBuilder builder, InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder.Name != "createElement")
            {
                result = null;
                return false;
            }

            if (!args.Any()) { throw new ArgumentNullException(nameof(args), "You need to specify alement name to create."); }

            var containsContent = args.Length > 1;

            var tag = (string)args[0];
            var content = containsContent ? args[1] : Enumerable.Empty<object>();
            result = builder.Create(tag).AddRange(content);
            return true;
        }
    }
}
