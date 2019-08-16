using System.Collections.Immutable;
using Reusable.Data;

namespace Reusable.Translucent
{
    public class RequestMethod : Option<RequestMethod>
    {
        public RequestMethod(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        public static readonly RequestMethod Get = CreateWithCallerName();

        public static readonly RequestMethod Post = CreateWithCallerName();

        public static readonly RequestMethod Put = CreateWithCallerName();

        public static readonly RequestMethod Delete = CreateWithCallerName();
    }
}