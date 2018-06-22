using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.SmartConfig.Data
{
    [PublicAPI]
    public interface ISetting
    {
        [NotNull]
        SoftString Name { get; }

        [CanBeNull]
        object Value { get; set; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class Setting : ISetting
    {
        [DebuggerStepThrough]
        public Setting([NotNull] SoftString name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public SoftString Name { [DebuggerStepThrough]get; }


        public object Value { [DebuggerStepThrough]get; [DebuggerStepThrough]set; }

        public static ISetting Create(SoftString name, object value) => new Setting(name) { Value = value };

        private string DebuggerDisplay => ToString();

        public override string ToString() => Name.ToString();

        public static implicit operator string(Setting setting) => setting.ToString();
    }
}