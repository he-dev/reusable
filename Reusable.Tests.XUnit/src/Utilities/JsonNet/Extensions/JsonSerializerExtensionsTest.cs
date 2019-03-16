using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.Utilities.JsonNet.Extensions;
using Xunit;

namespace Reusable.Tests.Utilities.JsonNet.Extensions
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
