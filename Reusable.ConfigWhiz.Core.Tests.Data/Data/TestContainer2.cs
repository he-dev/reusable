using Reusable.ConfigWhiz.Data.Annotations;

namespace Reusable.ConfigWhiz.Tests.Common.Data
{
    [DefaultDatastore("Datastore1")]
    public class TestContainer2
    {
        [Itemized]
        public string TestSetting2 { get; set; }
    }
}