using System.Collections.Immutable;
using JetBrains.Annotations;

namespace Reusable.SmartConfig
{
    public static class SqlServerColumnExtensions
    {
        [NotNull]
        public static string MapOrDefault(this IImmutableDictionary<SqlServerColumn, SoftString> mappings, SqlServerColumn column)
        {
            return
                mappings is null
                    ? column
                    : mappings.TryGetValue(column, out var mapping) && mapping
                        ? (string)mapping
                        : (string)column;
        }
    }
}