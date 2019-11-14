using System;
using System.IO;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.Translucent
{
    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class Response : IDisposable
    {
        public ResourceStatusCode StatusCode { get; set; }

        public object Body { get; set; }

        public IImmutableContainer Metadata { get; set; } = ImmutableContainer.Empty;

        // ReSharper disable once InconsistentNaming
        public static Response OK() => new Response { StatusCode = ResourceStatusCode.OK };
        
        public static Response NotFound() => new Response { StatusCode = ResourceStatusCode.NotFound };

        public void Dispose()
        {
            if (Body is Stream stream && Metadata?.GetItemOrDefault(Resource.MaxAge, TimeSpan.Zero) is var maxAge && maxAge == TimeSpan.Zero)
            {
                stream.Dispose();
            }
        }

        #region Properties

        private static readonly From<Response> This;

        public static readonly Selector<DateTime> CreateOn = This.Select(() => CreateOn);

        public static readonly Selector<DateTime> ModifiedOn = This.Select(() => ModifiedOn);

        public static readonly Selector<string> ActualName = This.Select(() => ActualName);

        #endregion
    }

    public enum ResourceStatusCode
    {
        // ReSharper disable once InconsistentNaming
        OK,
        NotFound
    }
}