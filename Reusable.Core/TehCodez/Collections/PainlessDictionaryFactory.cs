using System.Linq;
using System.Reflection;

namespace Reusable.Collections
{
    public static class PainlessDictionaryFactory
    {
        public static PainlessDictionary<string, object> CreateFromObject<T>(T obj)
        {
            var properties =
                typeof(T)
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => (Key: p.Name, Value: p.GetValue(obj)));

            return  new PainlessDictionary<string, object>().AddRange(properties);
        }
    }
}