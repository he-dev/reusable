using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Reflection;

namespace Reusable.Data.SqlClient
{
    // Based on
    // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections
    
    public interface ISqlSchema
    {
        [NotNull, ItemNotNull]
        IList<SqlTableSchema> GetTableSchemas([NotNull] SqlConnection sqlConnection, [NotNull] SqlTableSchema schemaRestriction);
        
        [NotNull, ItemNotNull]
        IList<SqlColumnSchema> GetColumnSchemas([NotNull] SqlConnection sqlConnection, [NotNull] SqlColumnSchema schemaRestriction);

        [NotNull, ItemNotNull]
        IList<SqlIdentityColumnSchema> GetIdentityColumns(SqlConnection connection, string schema, string table);
    }

    public class SqlSchema : ISqlSchema
    {
        private static readonly string GetIdentityColumnsQuery = ResourceReader<SqlSchema>.FindString(name => name.EndsWith($"{nameof(GetIdentityColumns)}.sql"));

        public IList<SqlTableSchema> GetTableSchemas(SqlConnection sqlConnection, SqlTableSchema schemaRestriction)
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

        public IList<SqlColumnSchema> GetColumnSchemas(SqlConnection sqlConnection, SqlColumnSchema schemaRestriction)
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

        public IList<SqlIdentityColumnSchema> GetIdentityColumns(SqlConnection connection, string schema, string table)
        {
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = GetIdentityColumnsQuery;
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
    }
}