using System;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog
{
    public delegate bool LogPredicate(Log log);
    
    public static class LogFilter
    {
        [NotNull]
        public static readonly LogPredicate Any = _ => true;

        [NotNull]
        public static LogPredicate Min([NotNull] this LogPredicate logPredicate, [NotNull] LogLevel logLevel)
        {
            if (logLevel == null) throw new ArgumentNullException(nameof(logLevel));

            return log => logPredicate(log) && log.Level() >= logLevel;
        }

        [NotNull]
        public static LogPredicate Contains([NotNull] this LogPredicate logPredicate, [NotNull] LogLevel logLevel)
        {
            if (logLevel == null) throw new ArgumentNullException(nameof(logLevel));

            return log => logPredicate(log) && logLevel.Contains(log.Level());
        }
        
        [NotNull]
        public static LogPredicate MessageRegex([NotNull] this LogPredicate logPredicate, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {

            return log => logPredicate(log) && logPredicate.LogRegex(l => l.Message() ?? string.Empty, pattern, regexOptions)(log);
        }
        
        [NotNull]
        public static LogPredicate NameRegex([NotNull] this LogPredicate logPredicate, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        {

            return log => logPredicate(log) && logPredicate.LogRegex(l => l.Name()?.ToString() ?? string.Empty, pattern, regexOptions)(log);
        }
        
        //[NotNull]
        //public static LogPredicate ScopeRegex([NotNull] this LogPredicate logPredicate, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.IgnoreCase)
        //{
        //    if (logPredicate == null) throw new ArgumentNullException(nameof(logPredicate));

        //    return log => logPredicate(log) && log.Scopes().Select(scope => scope.Name.ToString()).Any(scope => Regex.IsMatch(scope, pattern, regexOptions));
        //}

        [NotNull]
        public static LogPredicate LogRegex([NotNull] this LogPredicate logPredicate, Func<Log, string> getValue, [RegexPattern] string pattern, RegexOptions regexOptions = RegexOptions.None)
        {
            if (logPredicate == null) throw new ArgumentNullException(nameof(logPredicate));

            return log => logPredicate(log) && Regex.IsMatch(getValue(log), pattern, regexOptions);
        }
        
//        [NotNull]
//        public static LogFilterFunc StartsWith([NotNull] this LogFilterFunc filter, Func<Log, string> getValue, string value, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
//        {
//            if (filter == null) throw new ArgumentNullException(nameof(filter));
//
//            return log => filter(log) && getValue(log).StartsWith(value, stringComparison);
//        }
//        
//        [NotNull]
//        public static LogFilterFunc EndsWith([NotNull] this LogFilterFunc filter, Func<Log, string> getValue, string value, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase)
//        {
//            if (filter == null) throw new ArgumentNullException(nameof(filter));
//
//            return log => filter(log) && getValue(log).EndsWith(value, stringComparison);
//        }
        
        [NotNull]
        public static LogPredicate ContainsException([NotNull] this LogPredicate logPredicate)
        {
            if (logPredicate == null) throw new ArgumentNullException(nameof(logPredicate));

            return log => logPredicate(log) && log.Exception().IsNotNull();
        }
        
        [NotNull]
        public static LogPredicate Min([NotNull] this LogPredicate logPredicate, TimeSpan elapsed)
        {
            if (logPredicate == null) throw new ArgumentNullException(nameof(logPredicate));

            return log => logPredicate(log) && elapsed <= log.Elapsed();
        }               
    }
}