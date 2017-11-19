using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig.Tests.Common.Configurations
{
    //[Datastore("Datastore1")]
    public class TestContainer2
    {
        [Itemized]
        public string TestSetting2 { get; set; }
    }
}