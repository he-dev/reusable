namespace Reusable.Data.SqlClient
{
    public class SchemaRestriction
    {
        public string Catalog { get; set; }
        public string Owner { get; set; }
        public string Table { get; set; }
        public string TableType { get; set; }
        public static implicit operator string[] (SchemaRestriction schemaRestriction)
        {
            return new[]
            {
                schemaRestriction.Catalog,
                schemaRestriction.Owner,
                schemaRestriction.Table,
                schemaRestriction.TableType,
            };
        }
    }
}