namespace Reusable.Data.SqlClient
{
    // This is a custom class for the identity-column query.
    public class SqlIdentityColumnSchema
    {
        public string Name { get; set; }
        public object SeedValue { get; set; }
        public object IncrementValue { get; set; }
        public object LastValue { get; set; }
        public int UserTypeId { get; set; }
        public short MaxLength { get; set; }
        public bool IsComputed { get; set; }
    }
}