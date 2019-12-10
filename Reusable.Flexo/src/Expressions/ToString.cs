using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    /// <summary>
    /// Converts Input to string. Uses InvariantCulture by default.
    /// </summary>
    public class ToString : Extension<object, string>
    {
        public ToString() : base(default) { }

        public IExpression? Value
        {
            set => Arg = value;
        }

        public IExpression? Format { get; set; }

        protected override IEnumerable<string> ComputeMany(IImmutableContainer context)
        {
            var format = Format?.Invoke(context).ValueOrDefault<string>() ?? "{0}";
            return GetArg(context).Select(x => string.Format(CultureInfo.InvariantCulture, format, x));
        }
    }
}