using System;
using System.IO;
using System.Threading.Tasks;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.Translucent
{
    public class ResourceContext
    {
        public Request Request { get; set; }

        public Response Response { get; set; }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    public abstract class Resource : SelectorBuilder<Resource>
    {
        private Resource() { }

        #region Properties

        //private static readonly From<ResourceContext> This;

        /// <summary>
        /// Specifies the maximum amount of time a resource will be considered fresh.
        /// </summary>
        public static Selector<TimeSpan> MaxAge { get; } = Select(() => MaxAge);
        
        public static Selector<ITypeConverter> Converter { get; } = Select(() => Converter);
        
        public static Selector<Type> Type { get; } = Select(() => Type);

        #endregion
    }


    public delegate Task<Stream> CreateBodyStreamDelegate();
}