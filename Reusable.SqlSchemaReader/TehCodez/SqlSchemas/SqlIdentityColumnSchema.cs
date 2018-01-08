namespace Reusable.Data.SqlClient
{
    // This is a custom class for the identity-column query.
    public class SqlIdentityColumnSchema
    {
        public string Name { get; set; }
        public string SeedValue { get; set; }
        public string IncrementValue { get; set; }
        public string LastValue { get; set; }
        public string UserTypeId { get; set; }
        public string MaxLength { get; set; }
        public string IsComputed { get; set; }
    }
}