using Reusable.SmartConfig.Annotations;

namespace Reusable.SmartConfig.Tests.Common.Configurations
{
    [DefaultDatastore("Datastore1")]
    public class TestContainer1
    {
        public string TestSetting1 { get; set; }
    }
}