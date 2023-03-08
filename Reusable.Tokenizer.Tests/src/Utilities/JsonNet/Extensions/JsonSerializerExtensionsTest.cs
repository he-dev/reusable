using System.IO;
using Newtonsoft.Json;
using Xunit;

namespace Reusable.Utilities.JsonNet.Extensions
{
    public class JsonSerializerExtensionsTest
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer();
        
        [Fact]
        public void Serialize_can_serialize_object_to_specified_stream()
        {
            using (var serialized = new MemoryStream())
            {
                JsonSerializer.Serialize(serialized, "foo");
                var bytes = serialized.ToArray();
                Assert.Equal(5, bytes.Length);

                using (var ms2 = new MemoryStream(bytes))
                {
                    var foo = JsonSerializer.Deserialize<string>(ms2);
                    Assert.Equal("foo", foo);
                }
            }
        }
    }
}
