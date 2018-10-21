using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Extensions;

namespace Reusable.Utilities.SqlClient
{
    // Based on https://docs.microsoft.com/en-us/sql/t-sql/language-elements/transact-sql-syntax-conventions-transact-sql
    /// <summary>
    /// This class creates a multipart schema name.
    /// Unless specified otherwise, all Transact-SQL references to the name of a database object 
    /// can be a four-part name in the following form:
    ///  "server.[database].[schema].object",
    ///  "database.[schema].object",
    ///  "schema.object",
    ///  "object",
    /// </summary>
    [PublicAPI]
    public partial class SqlFourPartName
    {
        public const char OmittedNamePlaceholder = '.';

        public const char NameSeparator = '.';

        public SqlFourPartName([NotNull] string objectName)
        {
            ObjectName = objectName ?? throw new ArgumentNullException(nameof(objectName));
        }

        public SqlFourPartName([CanBeNull] string serverName, [CanBeNull] string databaseName, [CanBeNull] string schemaName, [NotNull] string objectName) : this(objectName)
        {
            ServerName = serverName;
            DatabaseName = databaseName;
            SchemaName = schemaName;
        }

        [CanBeNull]
        [AutoEqualityProperty]
        public string ServerName { get; set; }

        [CanBeNull]
        [AutoEqualityProperty]
        public string DatabaseName { get; set; }

        [CanBeNull]
        [AutoEqualityProperty]
        public string SchemaName { get; set; }

        [NotNull]
        [AutoEqualityProperty]
        public string ObjectName { get; }

        public string Render([NotNull] SqlConnection connection)
        {
            if (connection == null) { throw new ArgumentNullException(nameof(connection)); }

            return
                this
                    .Where(Conditional.IsNotNullOrEmpty)
                    .Select(name => name == OmittedNamePlaceholder.ToString() ? name : connection.CreateIdentifier(name) + NameSeparator)
                    .Join("")
                    // NameSeparator is attached to each valid name so there is one at the end that is too many.
                    // It's easier to remove it with TrimEnd than prevent adding it with an additional condition.
                    .TrimEnd(NameSeparator);
        }

        #region Convenience operators

        public static implicit operator SqlFourPartName((string serverName, string databaseName, string schemaName, string objectName) names)
        {
            return new SqlFourPartName(names.serverName, names.databaseName, names.schemaName, names.objectName);
        }

        public static implicit operator SqlFourPartName((string databaseName, string schemaName, string objectName) names)
        {
            return (null, names.databaseName, names.schemaName, names.objectName);
        }

        public static implicit operator SqlFourPartName((string schemaName, string objectName) names)
        {
            return (null, null, names.schemaName, names.objectName);
        }

        public static implicit operator SqlFourPartName(string objectName)
        {
            return (null, null, null, objectName);
        }

        #endregion
    }

    public partial class SqlFourPartName : IEnumerable<string>
    {
        public IEnumerator<string> GetEnumerator()
        {
            yield return ServerName;
            yield return DatabaseName ?? (ServerName is null ? null : OmittedNamePlaceholder.ToString());
            yield return SchemaName ?? (ServerName is null && DatabaseName is null ? null : OmittedNamePlaceholder.ToString());
            yield return ObjectName;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public partial class SqlFourPartName : IEquatable<SqlFourPartName>
    {
        public bool Equals(SqlFourPartName other)
        {
            return AutoEquality<SqlFourPartName>.Comparer.Equals(this, other);
        }

        public override bool Equals(object obj)
        {
            return obj is SqlFourPartName other && Equals(other);
        }

        public override int GetHashCode()
        {
            return AutoEquality<SqlFourPartName>.Comparer.GetHashCode(this);
        }
    }
}