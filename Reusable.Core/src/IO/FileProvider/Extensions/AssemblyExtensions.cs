using System.Reflection;

namespace Reusable.IO.Extensions
{
    public static class AssemblyExtensions
    {
        public static string ToPath(this Assembly assembly)
        {
            return assembly.GetName().Name.Replace(".", "\\");
        }
    }
}