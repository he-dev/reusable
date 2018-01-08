using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Data.SqlClient
{
    // This class provides utility methods for creating sql-schema types from data-tables.
    public abstract class SqlSchemaFactory
    {
        private static readonly ConcurrentDictionary<Type, Func<DataRow, object>> CreateSqlSchemaFuncs = new ConcurrentDictionary<Type, Func<DataRow, object>>();

        [NotNull]
        public static TSqlSchema Create<TSqlSchema>([NotNull] DataRow dataRow) where TSqlSchema : new()
        {
            var createSqlSchema = CreateSqlSchemaFuncs.GetOrAdd(typeof(TSqlSchema), CreateSqlSchemaFactoryFunc<TSqlSchema>);

            return (TSqlSchema)createSqlSchema(dataRow);
        }

        private static Func<DataRow, object> CreateSqlSchemaFactoryFunc<TSqlSchema>(Type type) where TSqlSchema : new()
        {
            var properties =
                (from property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                 where !property.IsDefined(typeof(NotMappedAttribute))
                 let sqlName = FormatName(property.Name)
                 select (property, sqlName)).ToList();

            return (row) =>
            {
                var sqlSchema = new TSqlSchema();
                foreach (var (property, sqlName) in properties.Where(x => !row[x.sqlName].Equals(DBNull.Value)))
                {
                    property.SetValue(sqlSchema, row[sqlName]);
                }
                return sqlSchema;
            };
        }

        private static string FormatName(string name)
        {
            return
                Regex
                    .Matches(name, "[A-Z][a-z]+")
                    .Cast<Match>()
                    .Select(m => m.Value.ToUpper())
                    .Join("_");
        }
    }
}