using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data.Repositories;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.DataStores.Internal;
using Reusable.Utilities.SqlClient;

namespace Reusable.SmartConfig.DataStores
{
    public class SqlServer : SettingDataStore
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        private IReadOnlyDictionary<string, object> _where = new Dictionary<string, object>();

        private SqlFourPartName _settingTableName;

        private SqlServerColumnMapping _columnMapping;

        public SqlServer(string nameOrConnectionString, ISettingConverter converter) : base(converter)
        {
            ConnectionString = ConnectionStringRepository.Default.GetConnectionString(nameOrConnectionString);

            SettingTableName = (DefaultSchema, DefaultTable);
            ColumnMapping = new SqlServerColumnMapping();
        }

        [NotNull]
        public string ConnectionString { get; }

        [NotNull]
        public SqlFourPartName SettingTableName
        {
            get => _settingTableName;
            set => _settingTableName = value ?? throw new ArgumentNullException(nameof(SettingTableName));
        }

        [NotNull]
        public SqlServerColumnMapping ColumnMapping
        {
            get => _columnMapping;
            set => _columnMapping = value ?? throw new ArgumentNullException(nameof(ColumnMapping));
        }

        public IReadOnlyDictionary<string, object> Where
        {
            get => _where;
            set => _where = value ?? throw new ArgumentNullException(nameof(Where));
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")] // it's fine to enumerate names multiple times
        protected override ISetting ReadCore(IEnumerable<SoftString> names)
        {
            return SqlHelper.Execute(ConnectionString, connection =>
            {
                using (var command = connection.CreateSelectCommand(this, names))
                {
                    //command.Prepare();

                    using (var settingReader = command.ExecuteReader())
                    {
                        var settings = new List<ISetting>();

                        while (settingReader.Read())
                        {
                            var result = new Setting((string)settingReader[ColumnMapping.Name])
                            {
                                Value = settingReader[ColumnMapping.Value],
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
            });
        }

        protected override void WriteCore(ISetting setting)
        {
            SqlHelper.Execute(ConnectionString, connection =>
            {
                using (var cmd = connection.CreateUpdateCommand(this, setting))
                {
                    //cmd.Prepare();
                    cmd.ExecuteNonQuery();
                }
            });            
        }
    }
}
