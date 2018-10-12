using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Data.Repositories;
using Reusable.SmartConfig.Data;
using Reusable.Utilities.SqlClient;

namespace Reusable.SmartConfig
{
    using Internal;
    
    public class SqlServer : SettingProvider
    {
        public const string DefaultSchema = "dbo";

        public const string DefaultTable = "Setting";

        private IReadOnlyDictionary<string, object> _where = new Dictionary<string, object>();

        private SqlFourPartName _settingTableName;

        private SqlServerColumnMapping _columnMapping;

        public SqlServer(string nameOrConnectionString, ISettingConverter converter) : base(new SettingNameFactory(), converter)
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

        protected override ISetting ReadCore(SettingName name)
        {
            return SqlHelper.Execute(
                ConnectionString, connection =>
                {
                    using (var command = connection.CreateSelectCommand(this, new[] { (SoftString)name }))
                    using (var settingReader = command.ExecuteReader())
                    {
                        if (settingReader.Read())
                        {
                            var setting = new Setting(
                                (string)settingReader[ColumnMapping.Name],
                                settingReader[ColumnMapping.Value]
                            );

                            if (settingReader.Read())
                            {
                                //                                throw CreateAmbiguousSettingException(names);
                            }

                            return setting;
                        }

                        return null;
                    }
                }
            );
        }

        protected override void WriteCore(ISetting setting)
        {
            SqlHelper.Execute(
                ConnectionString, connection =>
                {
                    using (var cmd = connection.CreateUpdateCommand(this, setting))
                    {
                        //cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                }
            );
        }
    }
}