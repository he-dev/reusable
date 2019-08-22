using System.Collections.Immutable;
using Reusable.Data;

namespace Reusable.Translucent
{
    public abstract class RequestMethod
    {
        public static readonly Option<RequestMethod> None = Option<RequestMethod>.None;
        public static readonly Option<RequestMethod> Get = Option<RequestMethod>.CreateWithCallerName();
        public static readonly Option<RequestMethod> Post = Option<RequestMethod>.CreateWithCallerName();
        public static readonly Option<RequestMethod> Put = Option<RequestMethod>.CreateWithCallerName();
        public static readonly Option<RequestMethod> Delete = Option<RequestMethod>.CreateWithCallerName();
    }
}