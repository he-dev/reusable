using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NLog.Targets;

namespace Reusable.Logging.NLog.Utils
{
    public static class DatabaseTargetQueryGenerator
    {
        public static void GenerateInsertQueries()
        {
            // The commandText attribute must conatain at least the table name. 
            // Each identifier must be enclosed in square brackets: [schemaName].[tableName].

            // https://msdn.microsoft.com/en-us/library/ms175874.aspx?f=255&MSPPError=-2147217396
            // https://regex101.com/r/19GjqR/1
            // h t t ps://regex101.com/delete/QObrsCHVjaX8gGCysAn6yApJ

            var tableNameMatcher = new Regex(@"^(\[(?<schemaName>[a-z_][a-z0-9_@$#]*)\].)?\[(?<tableName>[a-z_][a-z0-9_@$#]*)\]$");

            var databaseTargets = LogManager.Configuration.DatabaseTargets(databaseTarget => tableNameMatcher.IsMatch(databaseTarget.CommandText()));
            foreach (var databaseTarget in databaseTargets)
            {
                databaseTarget.CommandText = databaseTarget.GenerateInsertQuery();
            }
        }

        private static string GenerateInsertQuery(this DatabaseTarget databaseTarget)
        {
            const string insertQueryTemplate = "INSERT INTO {0}({1}) VALUES({2})";

            return string.Format(
                insertQueryTemplate,
                databaseTarget.CommandText(),
                string.Join(", ", databaseTarget.Parameters.Select(x => x.Name())),
                string.Join(", ", databaseTarget.Parameters.Select(x =>
                {
                    var sql =
                        x.Nullable()
                            ? string.Format("NULLIF({0}, '')", x.FullName())
                            : x.FullName();

                    // Rename the SqlParameter because otherwise SqlCommand will complain about it.
                    x.Name = x.FullName();

                    return sql;
                })));
        }
    }
}
