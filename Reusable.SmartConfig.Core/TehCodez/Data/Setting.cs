using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Data
{
    [PublicAPI]
    public interface ISetting
    {
        SoftString Name { get; set; }

        object Value { get; set; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class Setting : ISetting
    {
        public Setting() { }

        public Setting(SoftString name, object value)
        {
            Name = name;
            Value = value;
        }

        public SoftString Name
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }

        public object Value
        {
            [DebuggerStepThrough]
            get;
            [DebuggerStepThrough]
            set;
        }

        public static ISetting Create(SoftString name, object value) => new Setting(name, value);

        private string DebuggerDisplay => ToString();

        public override string ToString() => Name.ToString();

        public static implicit operator string(Setting setting) => setting.ToString();
    }
}