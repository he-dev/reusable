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
        [Fact]
        public void SerializeToBytes_string_bytes()
        {
            using (var memoryStream = new MemoryStream())
            {
                new JsonSerializer().Serialize(memoryStream, "foo");
                var bytes = memoryStream.ToArray();
                Assert.Equal(5, bytes.Length);

                using (var ms2 = new MemoryStream(bytes))
                {
                    var foo = new JsonSerializer().Deserialize<string>(ms2);
                    Assert.Equal("foo", foo);
                }
            }
        }
    }
}
