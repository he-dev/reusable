using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Reflection;

namespace Reusable.Utilities.SqlClient
{
    public static class SqlConnectionExtensions
    {
        [NotNull, ContractAnnotation("connection: null => halt")]
        public static string CreateIdentifier([NotNull] this SqlConnection connection, [NotNull] IEnumerable<string> names)
        {
            if (connection == null) throw new ArgumentNullException(nameof(connection));
            if (names == null) throw new ArgumentNullException(nameof(names));

            using (var commandBuilder = DbProviderFactories.GetFactory(connection).CreateCommandBuilder())
            {
                // ReSharper disable once PossibleNullReferenceException - commandBuilder is never null for SqlConnection.
                return names.Select(commandBuilder.QuoteIdentifier).Join(".");
            }
        }

        [NotNull, ContractAnnotation("connection: null => halt")]
        public static string CreateIdentifier([NotNull] this SqlConnection connection, [NotNull] params string[] names)
        {
            return connection.CreateIdentifier((IEnumerable<string>)names);
        }


        //public static async Task<IDisposable> ToggleIdentityInsertAsync(this SqlConnection connection, string schema, string table, IEnumerable<SoftString> columns)
        //{
        //    var identityColumns = connection.GetIdentityColumns(schema, table).Select(x => x.Name.ToSoftString()).ToList();
        //    var containsIdentityColumns = columns.Any(column => identityColumns.Contains(column));

        //    if (containsIdentityColumns)
        //    {
        //        var identifier = connection.CreateIdentifier(schema, table);
        //        using (var command = connection.CreateCommand())
        //        {
        //            command.CommandText = $"set identity_insert {identifier} on";
        //            await command.ExecuteNonQueryAsync();
        //        }

        //        return Disposable.Create(async () =>
        //        {
        //            using (var command = connection.CreateCommand())
        //            {
        //                command.CommandText = $"set identity_insert {identifier} off";
        //                await command.ExecuteNonQueryAsync();
        //            }
        //        });
        //    }

        //    return Disposable.Create(() => { });
        //}

        //public static async Task<ISet<SoftString>> GetPrimaryKeysAsync(this SqlConnection connection, string schema, string table)
        //{
        //    using (var cmd = connection.CreateCommand())
        //    {
        //        cmd.CommandText = Queries.GetPrimaryKeys;
        //        cmd.Parameters.AddWithValue("@schema", schema);
        //        cmd.Parameters.AddWithValue("@table", table);

        //        using (var reader = await cmd.ExecuteReaderAsync())
        //        {
        //            var keys = new HashSet<SoftString>();

        //            while (reader.Read())
        //            {
        //                keys.Add(await reader.GetFieldValueAsync<string>(0));
        //            }

        //            return keys;
        //        }
        //    }
        //}

        //private static class Queries
        //{
        //    public static readonly string GetPrimaryKeys = ResourceReader<SqlSchema>.FindString(name => name.EndsWith($"{nameof(GetPrimaryKeysAsync)}.sql"));
        //}
    }
}