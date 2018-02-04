using System.Text.RegularExpressions;
using NLog.Targets;

namespace Reusable.Utilities.ThirdParty.NLog
{
    public static class DatabaseParameterInfoExtensions
    {
        // https://regex101.com/r/wgoA3q/1
        // h t t ps://regex101.com/delete/BDmR7fAqwYQiT5DW5PKAJFAm

        private static readonly Regex ParamRegex = new Regex("^(?<Prefix>.)(?<Name>[a-z0-9_-]+)(?:[:](?<Null>null))?", RegexOptions.IgnoreCase);

        public static string Prefix(this DatabaseParameterInfo parameter) => ParamRegex.Match(parameter.Name).Groups["Prefix"].Value;

        public static string Name(this DatabaseParameterInfo parameter) => ParamRegex.Match(parameter.Name).Groups["Name"].Value;

        public static string FullName(this DatabaseParameterInfo parameter) => $"{parameter.Prefix()}{parameter.Name()}";

        public static bool Nullable(this DatabaseParameterInfo parameter) => ParamRegex.Match(parameter.Name).Groups["Null"].Success;
    }
}