using System.Linq;
using Reusable.Quickey;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Data;
using Xunit;

namespace Reusable.Translucent.config
{
    public class ConfigRequestBuilderTest
    {
        [Fact]
        public void Can_create_request()
        {
            var body = new object();
            var request = ConfigRequest.Create(ResourceMethod.Get, From<Map>.Select(x => x.City), body);
            Assert.Equal(ResourceMethod.Get, request.Method);
            Assert.Equal("Map.City", request.ResourceName);
            Assert.Same(body, request.Body);
            //Assert.Equal(typeof(string), request.Metadata.GetItem(ResourceProperties.DataType));
            Assert.Equal("ThisOne", request.ControllerName);
        }

        [UseType, UseMember]
        [Setting(Controller = "ThisOne")]
        [PlainSelectorFormatter]
        private class Map
        {
            public string City { get; set; }
        }
    }
}