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
            var request = ConfigRequestBuilder.CreateRequest(RequestMethod.Get, From<Map>.Select(x => x.City), body);
            Assert.Equal(RequestMethod.Get, request.Method);
            Assert.Equal(new UriString("config:settings?name=Map.City"), request.Uri.ToString());
            Assert.Same(body, request.Body);
            //Assert.Equal(typeof(string), request.Metadata.GetItem(ResourceProperties.DataType));
            Assert.Equal(new[] { "ThisOne" }, request.ControllerName.Tags);
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