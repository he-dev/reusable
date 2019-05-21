using System;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.Utilities.MSTest
{
    public interface ITypeAssert
    {
        Type Type { get; }
    }

    public class TypeAssert : ITypeAssert
    {
        public TypeAssert(Type type) => Type = type;
        public Type Type { get; }
    }

    public static class TypeAssertExtensions
    {
        public static void Implements<TExpected>(this ITypeAssert assert)
        {
            Assert.IsTrue(assert.Type.GetInterfaces().Contains(typeof(TExpected)), $"{assert.Type.ToPrettyString()} must implement {typeof(TExpected).ToPrettyString()}");
        }
    }
}