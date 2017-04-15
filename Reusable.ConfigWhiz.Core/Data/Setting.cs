using System.Diagnostics;

namespace Reusable.ConfigWhiz.Data
{
    public interface ISetting
    {
        SettingPath Path { get; set; }
        object Value { get; set; }
    }

    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public class Setting : ISetting
    {
        public SettingPath Path { [DebuggerStepThrough]get; [DebuggerStepThrough]set; }

        public object Value { [DebuggerStepThrough] get; [DebuggerStepThrough] set; }

        private string DebuggerDisplay => Path.ToString(SettingPathFormat.FullStrong, SettingPathFormatter.Instance);
    }
}