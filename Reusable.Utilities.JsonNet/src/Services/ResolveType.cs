using System;
using System.Collections.Immutable;
using Reusable.Exceptionize;

namespace Reusable.Utilities.JsonNet.Services
{
    public interface IResolveType
    {
        Type Invoke(string typeName);
    }
    
    public class ResolveType : IResolveType
    {
        public ResolveType(IImmutableDictionary<SoftString, Type> types)
        {
            Types = types;
        }

        private IImmutableDictionary<SoftString, Type> Types { get; }

        public Type Invoke(string typeName)
        {
            return
                Types.TryGetValue(typeName, out var type)
                    ? type
                    : Type.GetType(typeName, ignoreCase: true, throwOnError: false)
                      ?? throw DynamicException.Create("TypeNotFound", $"Could not resolve '{typeName}'.");
        }
    }
}