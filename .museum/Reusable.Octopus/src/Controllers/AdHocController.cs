using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;

namespace Reusable.Octopus.Controllers;

[PublicAPI]
public class AdHocController<TRequest> : ResourceController<TRequest> where TRequest : Request
{
    public AdHocController(string schema) : base(new[] { schema }) { }
    
    public Func<ResourceController<TRequest>, TRequest, Task<Response>>? OnRead { get; set; }
    public Func<ResourceController<TRequest>, TRequest, Task<Response>>? OnCreate { get; set; }
    public Func<ResourceController<TRequest>, TRequest, Task<Response>>? OnUpdate { get; set; }
    public Func<ResourceController<TRequest>, TRequest, Task<Response>>? OnDelete { get; set; }

    protected override Task<Response> ReadAsync(TRequest request) => OnRead?.Invoke(this, request) ?? throw NotSupportedException();
    protected override Task<Response> CreateAsync(TRequest request) => OnCreate?.Invoke(this, request) ?? throw NotSupportedException();
    protected override Task<Response> UpdateAsync(TRequest request) => OnUpdate?.Invoke(this, request) ?? throw NotSupportedException();
    protected override Task<Response> DeleteAsync(TRequest request) => OnDelete?.Invoke(this, request) ?? throw NotSupportedException();
}