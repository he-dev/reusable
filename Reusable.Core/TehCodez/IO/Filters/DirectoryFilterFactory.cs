using System;

namespace Reusable.IO
{
    public class DirectoryFilterFactory : PathFilterFactory
    {
        public static readonly IFilterFactory<string, string> Default = new DirectoryFilterFactory();

        protected override string FromWildcards(string pattern)
        {
            // Directories are matched at the \
            return @"\\" + base.FromWildcards(pattern);
        }

        protected override Func<string, bool> FromString(string pattern)
        {
            pattern = @"\" + pattern;
            return path => path.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}