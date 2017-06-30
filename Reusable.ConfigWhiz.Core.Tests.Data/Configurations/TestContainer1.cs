using Reusable.ConfigWhiz.Data.Annotations;

namespace Reusable.ConfigWhiz.Tests.Common.Data
{
    [DefaultDatastore("Datastore1")]
    public class TestContainer1
    {
        public string TestSetting1 { get; set; }
    }
}