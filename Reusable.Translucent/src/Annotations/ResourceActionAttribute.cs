using System;
using Reusable.Data;

namespace Reusable.Translucent.Annotations
{
    public abstract class ResourceActionAttribute : Attribute
    {
        protected ResourceActionAttribute(Reusable.Data.Option<RequestMethod> method) => Method = method;

        public Reusable.Data.Option<RequestMethod> Method { get; }
    }

    public class ResourceGetAttribute : ResourceActionAttribute
    {
        public ResourceGetAttribute() : base(RequestMethod.Get) { }
    }

    public class ResourcePostAttribute : ResourceActionAttribute
    {
        public ResourcePostAttribute() : base(RequestMethod.Post) { }
    }

    public class ResourcePutAttribute : ResourceActionAttribute
    {
        public ResourcePutAttribute() : base(RequestMethod.Put) { }
    }

    public class ResourceDeleteAttribute : ResourceActionAttribute
    {
        public ResourceDeleteAttribute() : base(RequestMethod.Delete) { }
    }
}