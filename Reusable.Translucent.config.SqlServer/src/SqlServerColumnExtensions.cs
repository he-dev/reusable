using System.Collections.Immutable;

namespace Reusable.Translucent
{
    public static class SqlServerColumnExtensions
    {
        public static string MapOrDefault(this IImmutableDictionary<SqlServerColumn, SoftString>? mappings, SqlServerColumn column)
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