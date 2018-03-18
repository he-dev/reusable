using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;

namespace Reusable.Utilities.AspNetCore
{
    public static class HostingEnvironmentExtensions
    {
        public static bool IsDevelopment(this IHostingEnvironment env, string name)
        {
            return env.IsEnvironment($"Development{name}");
        }        

        public static bool IsDevelopmentAny(this IHostingEnvironment env)
        {
            return env.EnvironmentName?.StartsWith("Development") ?? false;
        }
    }
}
