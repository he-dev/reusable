using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig.Tests.Common.Configurations
{
    [SettingName("MyContainer")]
    public class RenamedConfiguration
    {
        [SettingName("MySetting")]
        public string Bar { get; set; }
    }
}