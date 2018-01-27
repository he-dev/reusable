using JetBrains.Annotations;
using Reusable.Flawless;

namespace Reusable.SmartConfig.DataStores
{
    public class SqlServerColumnMapping
    {
        private static readonly IValidator<string> ColumnValidator =
            Validator
                .Create<string>()
                .IsNotValidWhen(column => column == null, ValidationOptions.StopOnFailure)
                .IsNotValidWhen(column => string.IsNullOrEmpty(column));

        private string _name;
        private string _value;

        public SqlServerColumnMapping()
        {
            Name = nameof(Name);
            Value = nameof(Value);
        }

        [NotNull]
        public string Name
        {
            get => _name;
            set => _name = value.ValidateWith(ColumnValidator).ThrowIfNotValid();
        }

        [NotNull]
        public string Value
        {
            get => _value;
            set => _value = value.ValidateWith(ColumnValidator).ThrowIfNotValid();
        }
    }
}