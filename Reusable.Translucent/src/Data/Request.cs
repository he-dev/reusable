using System;
using System.Text;
using System.Threading;

namespace Reusable.Translucent.Data
{
    public class Request : IDisposable
    {
        public UriString Uri { get; set; } = default!;// = new UriString($"{UriSchemes.Custom.IOnymous}:///");

        public Reusable.Data.Option<RequestMethod> Method { get; set; } = RequestMethod.None;

        /// <summary>
        /// Gets or sets the object that should be handled. Can be anything or a Stream.
        /// </summary>
        public object? Body { get; set; }

        #region Options

        public ControllerName ControllerName { get; set; } = ControllerName.Empty;

        public bool Required { get; set; } = true;

        public TimeSpan MaxAge { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public CancellationToken CancellationToken { get; set; }

        #endregion

        public static T CreateGet<T>(UriString uri, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Get, Uri = uri, Body = body };
        public static T CreatePost<T>(UriString uri, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Post, Uri = uri, Body = body };
        public static T CreatePut<T>(UriString uri, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Put, Uri = uri, Body = body };
        public static T CreateDelete<T>(UriString uri, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Delete, Uri = uri, Body = body };

        public void Dispose()
        {
            (Body as IDisposable)?.Dispose();
        }
    }
}