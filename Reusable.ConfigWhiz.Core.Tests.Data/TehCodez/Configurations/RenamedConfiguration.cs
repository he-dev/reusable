using Reusable.ConfigWhiz.Annotations;
using Reusable.ConfigWhiz.Data.Annotations;

namespace Reusable.ConfigWhiz.Tests.Common.Configurations
{
    [SettingName("MyContainer")]
    public class RenamedConfiguration
    {
        [SettingName("MySetting")]
        public string Bar { get; set; }
    }
}