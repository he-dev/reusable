using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Data;
using Reusable.Essentials.Extensions;

namespace Reusable.Octopus.Data;

using static RequestMethod;

public interface IItems
{
    IDictionary<string, object> Items { get; }
}

public class Request : IDisposable, IItems
{
    public string Schema { get; set; } = null!;

    public RequestMethod Method { get; set; } = None;

    public Trackable<string> ResourceName { get; } = new();

    public Trackable<object> Data { get; } = new();

    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

    public IControllerFilter? ControllerFilter { get; set; }

    public bool AllowControllerCaching { get; set; } = true;

    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    public static T Read<T>(string name, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Read, ResourceName = { name }, Data = { body } };
    public static T Create<T>(string name, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Create, ResourceName = { name }, Data = { body } };
    public static T Update<T>(string name, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Update, ResourceName = { name }, Data = { body } };
    public static T Delete<T>(string name, object? body = default) where T : Request, new() => new T { Method = RequestMethod.Delete, ResourceName = { name }, Data = { body } };

    public static implicit operator string(Request request) => request.ResourceName.Value;

    public void Dispose() => Data.Dispose();

    public class Builder<T> : IReadBuilder<T>, ICreateBuilder<T> where T : Request
    {
        public Builder(IResource resource, T request)
        {
            Resource = resource;
            Request = request;
        }

        private IResource Resource { get; }

        public T Request { get; }

        public Task<Response> InvokeAsync() => Resource.InvokeAsync(Request);
    }
}

public abstract record RequestMethodBuilder(IResource Resource)
{
    public record Read(IResource Resource) : RequestMethodBuilder(Resource);

    public record Create(IResource Resource) : RequestMethodBuilder(Resource);

    public record Update(IResource Resource) : RequestMethodBuilder(Resource);

    public record Delete(IResource Resource) : RequestMethodBuilder(Resource);
}

public interface IRequestBuilder<out T> where T : Request
{
    public T Request { get; }

    public Task<Response> InvokeAsync();
}

public interface IReadBuilder<out T> : IRequestBuilder<T> where T : Request { }

public interface ICreateBuilder<out T> : IRequestBuilder<T> where T : Request { }

public static class ResourceBuilderMethods
{
    public static IReadBuilder<T> Configure<T>(this IReadBuilder<T> builder, Action<T> action) where T : Request => builder.Also(b => action(b.Request));

    public static Request.Builder<T> Data<T>(this Request.Builder<T> builder, object data) where T : Request => builder.Also(b => b.Request.Data.Push(data));

    public static Request.Builder<T> Serializer<T>(this Request.Builder<T> builder, string name) where T : Request => builder.Also(b => b.Request.Items[nameof(Serializer)] = name);

    public static Request.Builder<T> As<T>(this Request.Builder<T> builder, Type type) where T : Request => builder.Also(b => b.Request.Items[nameof(As)] = type);
}