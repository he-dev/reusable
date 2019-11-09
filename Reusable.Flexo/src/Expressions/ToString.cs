using System.Globalization;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Converts Input to string. Uses InvariantCulture by default.
    /// </summary>
    public class ToString : ScalarExtension<string>
    {
        public ToString() : base(default) { }

        public IExpression Value { get => Arg; set => Arg = value; }

        public IExpression? Format { get; set; }

        protected override string ComputeValue(IImmutableContainer context)
        {
            var format = Format?.Invoke(context).ValueOrDefault<string>() ?? "{0}";
            return string.Format(CultureInfo.InvariantCulture, format, GetArg(context).Invoke(context).Value);
        }
    }
}