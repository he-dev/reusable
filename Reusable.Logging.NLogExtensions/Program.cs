using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog.Targets;

namespace NLogTargetBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = NLog.LogManager.Configuration;
            config.UpdateCommandText();

            var logger = NLog.LogManager.GetLogger("Program");
            logger.Debug("Hallo Log!");

            var dbTarget = config.AllTargets[2] as DatabaseTarget;
            dbTarget.CommandText = "asdlfj";
        }
    }

    static class NLogExtensions
    {
        public static void UpdateCommandText(this NLog.Config.LoggingConfiguration config)
        {
            var tableNameMatcher = new Regex(@"^(\[(?<schemaName>.+?)\].)?\[(?<tableName>.+?)\]$");

            var autoCommandTextDatabaseTargets =
                config.AllTargets
                    .OfType<DatabaseTarget>()
                    .Where(x => tableNameMatcher.IsMatch(x.CommandText()))
                    .Select(x => x);

            foreach (var databaseTarget in autoCommandTextDatabaseTargets)
            {
                databaseTarget.CommandText = databaseTarget.CreateCommandText();
            }
        }

        public static string CommandText(this DatabaseTarget databaseTarget)
        {
            return ((NLog.Layouts.SimpleLayout)databaseTarget.CommandText).OriginalText;
        }

        public static string CreateCommandText(this DatabaseTarget databaseTarget)
        {
            const string insertQueryTemplate = "INSERT INTO {0}({1}) VALUES({2})";

            return string.Format(
                    insertQueryTemplate,
                    databaseTarget.CommandText(),
                    string.Join(", ", databaseTarget.Parameters.Select(x => x.Name.TrimStart('@'))),
                    string.Join(", ", databaseTarget.Parameters.Select(x => x.Name)));
        }
    }
}
