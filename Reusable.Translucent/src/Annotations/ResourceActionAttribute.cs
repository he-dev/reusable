using System;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Annotations
{
    public abstract class ResourceActionAttribute : Attribute
    {
        protected ResourceActionAttribute(Reusable.Data.Option<ResourceMethod> method) => Method = method;

        public Reusable.Data.Option<ResourceMethod> Method { get; }
    }

    public class ResourceGetAttribute : ResourceActionAttribute
    {
        public ResourceGetAttribute() : base(ResourceMethod.Get) { }
    }

    public class ResourcePostAttribute : ResourceActionAttribute
    {
        public ResourcePostAttribute() : base(ResourceMethod.Post) { }
    }

    public class ResourcePutAttribute : ResourceActionAttribute
    {
        public ResourcePutAttribute() : base(ResourceMethod.Put) { }
    }

    public class ResourceDeleteAttribute : ResourceActionAttribute
    {
        public ResourceDeleteAttribute() : base(ResourceMethod.Delete) { }
    }
}