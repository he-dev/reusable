namespace Reusable.Translucent
{
    public class SqlServerColumn
    {
        private readonly string _name;

        private SqlServerColumn(string name) => _name = name;

        public static readonly SqlServerColumn Name = new SqlServerColumn(nameof(Name));

        public static readonly SqlServerColumn Value = new SqlServerColumn(nameof(Value));

        // todo - for future use
        //public static readonly SqlServerColumn ModifiedOn = new SqlServerColumn(nameof(ModifiedOn));
        //public static readonly SqlServerColumn CreatedOn = new SqlServerColumn(nameof(CreatedOn));

        public static implicit operator string(SqlServerColumn column) => column._name;
    }
}