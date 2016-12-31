using System.Text.RegularExpressions;
using NLog.Targets;

namespace Reusable.Logging.NLog.Utils
{
    public static class DatabaseParameterInfoExtensions
    {
        // https://regex101.com/r/wgoA3q/1
        // h t t ps://regex101.com/delete/BDmR7fAqwYQiT5DW5PKAJFAm

        private static readonly Regex ParamRegex = new Regex("^(?<prefix>.)(?<name>[a-z0-9_-]+)(?:[:](?<null>null))?", RegexOptions.IgnoreCase);

        public static string Prefix(this DatabaseParameterInfo parameter)
        {
            return ParamRegex.Match(parameter.Name).Groups["prefix"].Value;
        }

        public static string Name(this DatabaseParameterInfo parameter)
        {
            return ParamRegex.Match(parameter.Name).Groups["name"].Value;
        }

        public static string FullName(this DatabaseParameterInfo parameter)
        {
            return string.Format("{0}{1}", parameter.Prefix(), parameter.Name());
        }

        public static bool Nullable(this DatabaseParameterInfo parameter)
        {
            return ParamRegex.Match(parameter.Name).Groups["null"].Success;
        }
    }
}