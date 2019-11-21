using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Translucent
{
    [UseType, UseMember]
    [PlainSelectorFormatter]
    public class Request : IDisposable
    {
        public UriString Uri { get; set; } // = new UriString($"{UriSchemes.Custom.IOnymous}:///");

        public Option<RequestMethod> Method { get; set; } = RequestMethod.None;

        public object? Body { get; set; }

        #region Options

        public string? ControllerId { get; set; }

        public ISet<SoftString>? ControllerTags { get; set; }

        public bool Required { get; set; } = true;

        public TimeSpan MaxAge { get; set; }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public CancellationToken CancellationToken { get; set; }

        #endregion

        public static T Create<T>(Option<RequestMethod> method, UriString uri, object? body = default, Action<T>? requestAction = default) where T : Request, new()
        {
            var request = new T { Method = method, Uri = uri, Body = body };
            requestAction?.Invoke(request);
            return request;
        }

        public void Dispose()
        {
            (Body as IDisposable)?.Dispose();
        }

        #region Methods

        public class Get : Request
        {
            public Get(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Get;
            }
        }

        public class Post : Request
        {
            public Post(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Post;
            }
        }

        public class Put : Request
        {
            public Put(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Put;
            }
        }

        public class Delete : Request
        {
            public Delete(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Delete;
            }
        }

        #endregion
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SchemeAttribute : Attribute, IEnumerable<SoftString>
    {
        private readonly IEnumerable<string> _schemes;
        public SchemeAttribute(params string[] schemes) => _schemes = schemes.AsEnumerable();
        public IEnumerator<SoftString> GetEnumerator() => _schemes.Select(s => s.ToSoftString()).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _schemes.GetEnumerator();
    }
}