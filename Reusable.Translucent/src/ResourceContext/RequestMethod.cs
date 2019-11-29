using Reusable.Data;

namespace Reusable.Translucent
{
    public abstract class RequestMethod
    {
        public static readonly Reusable.Data.Option<RequestMethod> None = Reusable.Data.Option<RequestMethod>.None;
        public static readonly Reusable.Data.Option<RequestMethod> Get = Reusable.Data.Option<RequestMethod>.CreateWithCallerName();
        public static readonly Reusable.Data.Option<RequestMethod> Post = Reusable.Data.Option<RequestMethod>.CreateWithCallerName();
        public static readonly Reusable.Data.Option<RequestMethod> Put = Reusable.Data.Option<RequestMethod>.CreateWithCallerName();
        public static readonly Reusable.Data.Option<RequestMethod> Delete = Reusable.Data.Option<RequestMethod>.CreateWithCallerName();
    }
}