using System.Collections.Generic;

namespace Reusable.Flexo
{
    /// <summary>
    /// Provides a mapping expression from bool to double.
    /// </summary>
    public class ToDouble : Switch<bool>
    {
        public ToDouble(string name) : base(name)
        {
            Cases = new List<SwitchCase>
            {
                new SwitchCase
                {
                    When = Constant.True,
                    Body = Double.One
                },
                new SwitchCase
                {
                    When = Constant.False,
                    Body = Double.Zero
                },
                new SwitchCase
                {
                    When = Constant.Null,
                    Body = Double.Zero
                }
            };

            Default = new Throw("SwitchValueOutOfRange")
            {
                Message = Constant.FromValue("Message", $"{nameof(ToDouble)} value must be either 'true', 'false' or 'null'.")
            };
        }

        public ToDouble() : this(nameof(ToDouble)) { }
    }
}