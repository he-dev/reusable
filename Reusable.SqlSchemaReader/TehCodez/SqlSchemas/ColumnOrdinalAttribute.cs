using System;

namespace Reusable.Utilities.SqlClient
{
    /// <inheritdoc />
    /// <summary>
    /// Sets the (zero-based) position of the column in the DataColumnCollection collection.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class ColumnOrdinalAttribute : Attribute
    {
        private readonly int _ordinal;

        // This is a positional argument
        public ColumnOrdinalAttribute(int ordinal)
        {
            if (ordinal < 0) throw new ArgumentOutOfRangeException(paramName: nameof(ordinal), message: "Column ordinal must be >= 0.");
            _ordinal = ordinal;
        }

        public static implicit operator int(ColumnOrdinalAttribute attribute) => attribute?._ordinal ?? throw new ArgumentNullException(nameof(attribute));
    }
}