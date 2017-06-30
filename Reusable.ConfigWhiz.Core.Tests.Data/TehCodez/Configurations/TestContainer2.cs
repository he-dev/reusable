using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig.Tests.Common.Configurations
{
    [DefaultDatastore("Datastore1")]
    public class TestContainer2
    {
        [Itemized]
        public string TestSetting2 { get; set; }
    }
}