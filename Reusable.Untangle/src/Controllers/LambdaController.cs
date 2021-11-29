using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public class LambdaController<TRequest> : ResourceController<TRequest> where TRequest : Request
    {
        public Func<TRequest, Task<Response>>? OnCreate { get; set; }
        public Func<TRequest, Task<Response>>? OnRead { get; set; }
        public Func<TRequest, Task<Response>>? OnUpdate { get; set; }
        public Func<TRequest, Task<Response>>? OnDelete { get; set; }

        public override Task<Response> CreateAsync(TRequest request) => OnCreate?.Invoke(request) ?? throw NotSupportedException();
        public override Task<Response> ReadAsync(TRequest request) => OnRead?.Invoke(request) ?? throw NotSupportedException();
        public override Task<Response> UpdateAsync(TRequest request) => OnUpdate?.Invoke(request) ?? throw NotSupportedException();
        public override Task<Response> DeleteAsync(TRequest request) => OnDelete?.Invoke(request) ?? throw NotSupportedException();
    }
}