using System;

namespace Reusable.IO
{
    public class FileFilterFactory : PathFilterFactory
    {
        public static readonly IFilterFactory<string, string> Default = new FileFilterFactory();

        protected override string FromWildcards(string pattern)
        {
            // Files are matched at the end of the string.
            return base.FromWildcards(pattern) + "$";
        }

        protected override Func<string, bool> FromString(string pattern)
        {
            return path => path.EndsWith(pattern, StringComparison.OrdinalIgnoreCase);
        }
    }
}