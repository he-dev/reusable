using System;
using System.Configuration;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Data
{
    public static class AppConfigHelper
    {
        public static string GetConnectionString(string nameOrConnectionString)
        {
            return Regex.Match(nameOrConnectionString, @"\Aname=(?<name>.+)", RegexOptions.IgnoreCase) switch
            {
                {Success: true} match => ConfigurationManager.ConnectionStrings[match.Groups["name"].Value]?.ConnectionString,
                _ => nameOrConnectionString
            } ?? throw DynamicException.Create($"ConnectionStringNotFound", $"Could not find connection string {nameOrConnectionString.QuoteWith("'")}.");
        }
    }
}