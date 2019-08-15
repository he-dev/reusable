using System.Linq;
using Reusable.Data;
using Reusable.Quickey;
using Reusable.Translucent.Annotations;
using Xunit;

namespace Reusable.Translucent.config
{
    public class SettingRequestBuilderTest
    {
        [Fact]
        public void Can_create_request()
        {
            var body = new object();
            var request = SettingRequestBuilder.CreateRequest(RequestMethod.Get, From<Map>.Select(x => x.City), body);
            Assert.Equal(RequestMethod.Get, request.Method);
            Assert.Equal(new UriString("config:settings?name=Map.City"), request.Uri.ToString());
            Assert.Same(body, request.Body);
            //Assert.Equal(typeof(string), request.Metadata.GetItem(ResourceProperties.DataType));
            Assert.Equal(new[] { "ThisOne" }, request.Metadata.GetItem(ResourceController.Tags).Select(x => x.ToString()));
        }

        [UseType, UseMember]
        [Resource(Provider = "ThisOne")]
        [PlainSelectorFormatter]
        private class Map
        {
            public string City { get; set; }
        }
    }
}