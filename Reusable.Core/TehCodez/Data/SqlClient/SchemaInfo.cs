using System;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Reusable.Data.SqlClient
{
    public abstract class SchemaInfo
    {
        protected SchemaInfo(DataRow row)
        {
            var properties =
                GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => new
                    {
                        Property = p,
                        Name = CustomAttributeExtensions.GetCustomAttribute<SchemaColumnNameAttribute>((MemberInfo) p)
                    })
                    .Where(x => x.Name != null);

            foreach (var x in properties)
            {
                var value = row[x.Name];
                x.Property.SetValue(this, DBNull.Value.Equals(value) ? null : value);
            }
        }
    }
}