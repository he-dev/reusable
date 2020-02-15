using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Reusable.Data;

namespace Reusable.Translucent.Data
{
    public class Request : IDisposable
    {
        public Option<ResourceMethod> Method { get; set; } = ResourceMethod.None;

        public string ResourceName { get; set; } = default!;


        /// <summary>
        /// Gets or sets the object that should be handled. Can be anything or a Stream.
        /// </summary>
        public object? Body { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

        public ControllerName ControllerName { get; set; } = ControllerName.Empty;

        public CancellationToken CancellationToken { get; set; }

        public static T CreateGet<T>(string uri, object? body = default) where T : Request, new() => new T { Method = ResourceMethod.Get, ResourceName = uri, Body = body };
        public static T CreatePost<T>(string uri, object? body = default) where T : Request, new() => new T { Method = ResourceMethod.Post, ResourceName = uri, Body = body };
        public static T CreatePut<T>(string uri, object? body = default) where T : Request, new() => new T { Method = ResourceMethod.Put, ResourceName = uri, Body = body };
        public static T CreateDelete<T>(string uri, object? body = default) where T : Request, new() => new T { Method = ResourceMethod.Delete, ResourceName = uri, Body = body };

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