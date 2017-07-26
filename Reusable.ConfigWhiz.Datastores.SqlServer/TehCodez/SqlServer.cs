using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Datastores
{
    public class SqlServer : Datastore
    {
        private readonly string _connectionString;

        private string _schema = "dbo";
        private string _table = "Setting";
        private IImmutableDictionary<string, object> _where = ImmutableDictionary<string, object>.Empty;

        public SqlServer(string nameOrConnectionString) : base(new[] { typeof(string) })
        {
            var connectionStringName = nameOrConnectionString.ExtractConnectionStringName();
            _connectionString =
                connectionStringName.IsNullOrEmpty()
                    ? nameOrConnectionString
                    : AppConfigRepository.Deafult.GetConnectionString(connectionStringName);

            if (_connectionString.IsNullOrEmpty()) { throw new ArgumentNullException(nameof(nameOrConnectionString)); }
        }

        public string Schema
        {
            get => _schema;
            set => _schema = value ?? throw new ArgumentNullException(nameof(Schema));
        }

        public string Table
        {
            get => _table;
            set => _table = value ?? throw new ArgumentNullException(nameof(Table));
        }

        public IImmutableDictionary<string, object> Where
        {
            get => _where;
            set => _where = value ?? throw new ArgumentNullException(nameof(Where));
        }

        protected override ISetting ReadCore(IEnumerable<CaseInsensitiveString> names)
        {
            using (var connection = OpenConnection())
            using (var command = connection.CreateSelectCommand(this, names))
            {
                command.Prepare();

                using (var settingReader = command.ExecuteReader())
                {
                    var settings = new List<ISetting>();

                    while (settingReader.Read())
                    {
                        var result = new Setting
                        {
                            Name = (string)settingReader[nameof(ISetting.Name)],
                            Value = settingReader[nameof(ISetting.Value)],
                        };
                        settings.Add(result);
                    }

                    return
                        (from name in names
                         from setting in settings
                         where name.Equals(setting.Name)
                         select setting).FirstOrDefault(Conditional.IsNotNull);
                }
            }
        }

        protected override void WriteCore(ISetting setting)
        {
            using (var connection = OpenConnection())
            using (var transaction = connection.BeginTransaction())
            using (var cmd = connection.CreateUpdateCommand(this, setting))
            {
                cmd.Transaction = transaction;
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }
        }

        private SqlConnection OpenConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
    }

    public class ColumnConfigurationNotFoundException : Exception
    {
        internal ColumnConfigurationNotFoundException(string column)
        {
            Column = column;
        }

        public string Column { get; set; }

        public override string Message => $"\"{Column}\" column configuration not found. Ensure that it is set via the \"{nameof(SqlServer)}\" builder.";
    }

    public static class TableMetadataExtensions
    {
        public static TableMetadata<SqlDbType> AddNameColumn(this TableMetadata<SqlDbType> tableMetadata, SqlDbType dbType = SqlDbType.NVarChar, int length = 200)
        {
            return tableMetadata.AddColumn(EntityProperty.Name, dbType, length);
        }

        public static TableMetadata<SqlDbType> AddValueColumn(this TableMetadata<SqlDbType> tableMetadata, SqlDbType dbType = SqlDbType.NVarChar, int length = -1)
        {
            return tableMetadata.AddColumn(EntityProperty.Value, dbType, length);
        }
    }
}
