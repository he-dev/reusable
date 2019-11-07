using System.Globalization;
using Reusable.Data;

namespace Reusable.Flexo
{
    /// <summary>
    /// Converts Input to string. Uses InvariantCulture by default.
    /// </summary>
    public class ToString : ScalarExtension<string>
    {
        public ToString() : base(default, nameof(ToString)) { }

        public IExpression Value { get => ThisInner; set => ThisInner = value; }

        public IExpression? Format { get; set; }

        protected override string ComputeValue(IImmutableContainer context)
        {
            var format = Format?.Invoke(context).ValueOrDefault<string>() ?? "{0}";
            return string.Format(CultureInfo.InvariantCulture, format, This(context).Invoke(context).Value);
        }
    }
}