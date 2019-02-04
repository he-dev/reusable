using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Reusable.Deception
{
    public class PhantomExceptionFilter
    {
        //private readonly Lazy<Func<IEnumerable<string>, bool>> _matches;

        // public PhantomExceptionFilter()
        // {
        //     _matches = Lazy.Create<Func<IEnumerable<string>, bool>>(() =>
        //     {
        //         var patterns = new[] { Namespace, Type, Member, Id }.Select(ToPattern).ToList();
        //
        //         return names => names.Zip(patterns, Matches).All(m => m);
        //
        //         bool Matches(string value, string pattern)
        //         {
        //             var hasValue = !string.IsNullOrWhiteSpace(value);
        //             var hasPattern = !string.IsNullOrWhiteSpace(pattern);
        //
        //             return !hasPattern || (hasValue && Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase));
        //         }
        //
        //         string ToPattern(string value) => string.IsNullOrWhiteSpace(value) ? default : (IsRegex(value) ? value.Trim('/') : Regex.Escape(value));
        //
        //         bool IsRegex(string value) => (value.StartsWith("/") && value.EndsWith("/"));
        //     });
        // }

        public string Namespace { get; set; }

        public string Type { get; set; }

        public string Member { get; set; }

        public string Id { get; set; }

        public bool Matches(IEnumerable<StackFrameInfo> stackFrames)
        {
            var patterns = new[] { Namespace, Type, Member, Id };

            return stackFrames.Any(stackFrame => patterns.Zip(stackFrame, (pattern, value) => Matches(value, pattern)).All(m => m));

            bool Matches(string value, string pattern)
            {
                var hasValue = !string.IsNullOrWhiteSpace(value);
                var hasPattern = !string.IsNullOrWhiteSpace(pattern);

                return !hasPattern || (hasValue && Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase));
            }
        }

        public static implicit operator PhantomExceptionFilter((string Namespace, string Type, string Member, string Id) filter)
        {
            return new PhantomExceptionFilter
            {
                Namespace = filter.Namespace,
                Type = filter.Type,
                Member = filter.Member,
                Id = filter.Id
            };
        }
    }
}