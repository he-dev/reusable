using Reusable.ConfigWhiz.Data.Annotations;

namespace Reusable.ConfigWhiz.Tests.Common.Data
{
    [SettingName("MyContainer")]
    public class Renamed
    {
        [SettingName("MySetting")]
        public string Bar { get; set; }
    }
}