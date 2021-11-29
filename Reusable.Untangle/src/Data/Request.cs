using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Reusable.Translucent.Data
{
    using static ResourceMethod;
    
    public abstract class Request : IDisposable
    {
        public ResourceMethod Method { get; set; } = None;

        public string ResourceName { get; set; } = default!;

        /// <summary>
        /// Gets or sets the object that should be handled. Can be anything or a Stream.
        /// </summary>
        public object? Body { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

        public SoftString? ControllerName { get; set; }

        public ISet<SoftString> ControllerTags { get; set; } = new SortedSet<SoftString>();

        public CancellationToken CancellationToken { get; set; }

        public static T Read<T>(string uri, object? body = default) where T : Request, new() => new T { Method = ResourceMethod.Read, ResourceName = uri, Body = body };
        public static T Create<T>(string uri, object? body = default) where T : Request, new() => new T { Method = ResourceMethod.Create, ResourceName = uri, Body = body };
        public static T Update<T>(string uri, object? body = default) where T : Request, new() => new T { Method = ResourceMethod.Update, ResourceName = uri, Body = body };
        public static T Delete<T>(string uri, object? body = default) where T : Request, new() => new T { Method = ResourceMethod.Delete, ResourceName = uri, Body = body };

        public void Dispose()
        {
            (Body as IDisposable)?.Dispose();
        }
    }

    public static class RequestHelper
    {
        public static T GetItemOrDefault<T>(this Request request, string name, T defaultValue)
        {
            return
                request.Items.TryGetValue(name, out var value) && value is T result
                    ? result
                    : defaultValue;
        }
    }
}