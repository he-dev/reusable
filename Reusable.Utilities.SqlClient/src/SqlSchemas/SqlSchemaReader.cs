using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Translucent;

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
            var resources = ResourceSquid.Builder.UseEmbeddedFiles(typeof(SqlSchemaReader), @"Reusable\Utilities\SqlClient\sql").Build();
            GetIdentityColumnSchemasQuery = resources.ReadTextFile($"sql\\{nameof(GetIdentityColumnSchemas)}.sql");
        }

        public static IList<SqlTableSchema> GetTableSchemas(this SqlConnection sqlConnection, SqlTableSchema schemaRestriction)
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
        public static IList<SqlColumnSchema> GetColumnSchemas(this SqlConnection sqlConnection, SqlColumnSchema schemaRestriction)
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

        public static IList<SqlIdentityColumnSchema> GetIdentityColumnSchemas(this SqlConnection connection, string schema, string table)
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
        public static IList<(SoftString Name, Type Type)> GetColumnFrameworkTypes(this SqlConnection connection, [NotNull] string schema, [NotNull] string table)
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