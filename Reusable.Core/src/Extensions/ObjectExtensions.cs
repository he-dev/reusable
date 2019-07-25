using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class ObjectExtensions
    {
//        public static IEnumerable<KeyValuePair<string, object>> EnumerateProperties<T>(this T obj)
//        {
//            if (obj is IDictionary<string, object> dictionary)
//            {
//                foreach (var item in dictionary)
//                {
//                    yield return item;
//                }
//
//                yield break;
//            }
//
//            if (!obj.GetType().Name.StartsWith("<>f__AnonymousType1"))
//            {
//                DynamicException.Factory.CreateDynamicException("ObjectType", "Object must be an anonymous type. Try new { ... }");
//            }
//
//            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
//            foreach (var property in properties)
//            {
//                yield return new KeyValuePair<string, object>(property.Name, property.GetValue(obj));
//            }
//        }

        public static Task<T> ToTask<T>(this T obj) => Task.FromResult<T>(obj);

        [Pure]
        [NotNull]
        [MustUseReturnValue]
        public static T DecorateWith<T>(this T decorable, Func<T, T> createDecorator)
        {
            return createDecorator(decorable);
        }
    }
}