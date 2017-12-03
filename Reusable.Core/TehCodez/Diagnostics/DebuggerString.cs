using System.Linq;
using System.Reflection;
using System.Text;
using Reusable.Extensions;
using System.Linq.Custom;

namespace Reusable.Diagnostics
{
    public static class DebuggerString
    {
        public static string Create<T>(object obj)
        {
            var properties =
                obj.GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => $"{p.Name} = {p.GetValue(obj).ToString().QuoteWith("'")}")
                    .Join(" ");

            return $"{typeof(T).Name}: {properties}";
        }
    }
}
