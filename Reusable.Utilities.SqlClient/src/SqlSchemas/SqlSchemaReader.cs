using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus;
using Reusable.Octopus.Controllers;

namespace Reusable.Utilities.SqlClient.SqlSchemas
{
    // Based on
    // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections

    [PublicAPI]
    public static class SqlSchemaReader
    {
        private static readonly string GetIdentityColumnSchemasQuery;


        static SqlSchemaReader()
        {
            var resource = new Resource(new[] { new EmbeddedFileController(@"Reusable\Utilities\SqlClient\sql", typeof(SqlSchemaReader).Assembly) });

            GetIdentityColumnSchemasQuery = resource.ReadTextFile($"sql\\{nameof(GetIdentityColumnSchemas)}.sql");
        }

        public static List<SqlTableSchema> GetTableSchemas(this SqlConnection sqlConnection, SqlTableSchema schemaRestriction)
        {
            if (sqlConnection == null) throw new ArgumentNullException(nameof(sqlConnection));
            if (schemaRestriction == null) throw new ArgumentNullException(nameof(schemaRestriction));

            using (var schema = sqlConnection.GetSchema(SqlSchemaCollection.Tables, schemaRestriction))
            {
                return
                    schema
                        .AsEnumerable()
                        .Select(SqlSchemaFactory.Create<SqlTableSchema>)
                        .ToList();
            }
        }

        /// <summary>
        /// Gets column schemas ordered by their ordinal-position.
        /// </summary>
        public static List<SqlColumnSchema> GetColumnSchemas(this SqlConnection sqlConnection, SqlColumnSchema schemaRestriction)
        {
            if (sqlConnection == null) throw new ArgumentNullException(nameof(sqlConnection));
            if (schemaRestriction == null) throw new ArgumentNullException(nameof(schemaRestriction));

            using (var schema = sqlConnection.GetSchema(SqlSchemaCollection.Columns, schemaRestriction))
            {
                return
                    schema
                        .AsEnumerable()
                        .Select(SqlSchemaFactory.Create<SqlColumnSchema>)
                        .OrderBy(x => x.OrdinalPosition)
                        .ToList();
            }
        }

        public static List<SqlIdentityColumnSchema> GetIdentityColumnSchemas(this SqlConnection connection, string schema, string table)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = GetIdentityColumnSchemasQuery;
                cmd.Parameters.AddWithValue("@schema", schema);
                cmd.Parameters.AddWithValue("@table", table);

                using (var reader = cmd.ExecuteReader())
                {
                    var identityColumns = new DataTable("IdentityColumns");
                    identityColumns.Load(reader);
                    return
                        identityColumns
                            .AsEnumerable()
                            .Select(SqlSchemaFactory.Create<SqlIdentityColumnSchema>)
                            .ToList();
                }
            }
        }

        [NotNull]
        public static List<(SoftString Name, Type Type)> GetColumnFrameworkTypes(this SqlConnection connection, [NotNull] string schema, [NotNull] string table)
        {
            if (schema == null) throw new ArgumentNullException(nameof(schema));
            if (table == null) throw new ArgumentNullException(nameof(table));

            return connection.GetColumnSchemas(new SqlColumnSchema
                {
                    TableSchema = schema,
                    TableName = table
                })
                .Select((column, ordinal) => (column.ColumnName.ToSoftString(), column.FrameworkType))
                .ToList();
        }
    }
}