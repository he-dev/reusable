using Reusable.Data;

namespace Reusable.Translucent
{
    public abstract class ResourceMethod
    {
        public static readonly Option<ResourceMethod> None = Option<ResourceMethod>.None;
        public static readonly Option<ResourceMethod> Get = Option<ResourceMethod>.CreateWithCallerName();
        public static readonly Option<ResourceMethod> Post = Option<ResourceMethod>.CreateWithCallerName();
        public static readonly Option<ResourceMethod> Put = Option<ResourceMethod>.CreateWithCallerName();
        public static readonly Option<ResourceMethod> Delete = Option<ResourceMethod>.CreateWithCallerName();
    }
}