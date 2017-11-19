using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data.Repositories;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;
using Reusable.SmartConfig.Internal;

namespace Reusable.SmartConfig
{
    public class SqlServer : Datastore
    {
        private static readonly ConnectionStringRepository ConnectionStringRepository = new ConnectionStringRepository();
        private readonly string _connectionString;

        private string _schema = "dbo";
        private string _table = "Setting";
        private IReadOnlyDictionary<string, object> _where = new Dictionary<string, object>();

        public SqlServer(string nameOrConnectionString) : base(Enumerable.Empty<Type>())
        {
            _connectionString =
                ConnectionStringRepository.GetConnectionString(nameOrConnectionString) ??
                throw new ArgumentNullException(
                    paramName: nameof(nameOrConnectionString),
                    message: $"Connection string '{nameOrConnectionString}' not found.");
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

        public IReadOnlyDictionary<string, object> Where
        {
            get => _where;
            set => _where = value ?? throw new ArgumentNullException(nameof(Where));
        }

        protected override ISetting ReadCore(IEnumerable<SoftString> names)
        {
            using (var connection = new SqlConnection(_connectionString).Then(c => c.Open()))
            using (var command = connection.CreateSelectCommand(this, names))
            {
                //command.Prepare();

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
            using (var connection = new SqlConnection(_connectionString).Then(c => c.Open()))
            using (var transaction = connection.BeginTransaction())
            using (var cmd = connection.CreateUpdateCommand(this, setting))
            {
                cmd.Transaction = transaction;
                //cmd.Prepare();
                cmd.ExecuteNonQuery();
                transaction.Commit();
            }
        }
    }

    //public class WhereDictionary : IDictionary<string, object>
    //{
    //    private static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;

    //    private readonly IDictionary<string, object> _where;

    //    public WhereDictionary() : this(new Dictionary<string, object>(Comparer)) { }

    //    public WhereDictionary(IDictionary<string, object> dictionary)
    //    {
    //        _where = new Dictionary<string, object>(dictionary, Comparer);
    //    }

    //    public object this[string key]
    //    {
    //        get => _where[key];
    //        set => _where[key] = value;
    //    }

    //    public int Count => _where.Count;

    //    public ICollection<string> Keys => _where.Keys;

    //    public ICollection<object> Values => _where.Values;

    //    public bool IsReadOnly => _where.IsReadOnly;

    //    public void Add(string key, [NotNull] object value)
    //    {
    //        if (key.IsNullOrEmpty()) throw new ArgumentNullException(nameof(key));
    //        if (value == null) throw new ArgumentNullException(nameof(value));

    //        _where.Add(key, value);
    //    }

    //    public void Add(KeyValuePair<string, object> item)
    //    {
    //        _where.Add(item.Key, item.Value);
    //    }

    //    public void Clear()
    //    {
    //        _where.Clear();
    //    }

    //    public bool Contains(KeyValuePair<string, object> item)
    //    {
    //        return _where.Contains(item);
    //    }

    //    public bool ContainsKey(string key)
    //    {
    //        return _where.ContainsKey(key);
    //    }

    //    public bool Remove(string key)
    //    {
    //        return _where.Remove(key);
    //    }

    //    public bool Remove(KeyValuePair<string, object> item)
    //    {
    //        return _where.Remove(item);
    //    }

    //    public bool TryGetValue(string key, out object value)
    //    {
    //        return _where.TryGetValue(key, out value);
    //    }

    //    public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    //    {
    //        _where.CopyTo(array, arrayIndex);
    //    }

    //    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    //    {
    //        return _where.GetEnumerator();
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return ((IEnumerable)_where).GetEnumerator();
    //    }
    //}
}
