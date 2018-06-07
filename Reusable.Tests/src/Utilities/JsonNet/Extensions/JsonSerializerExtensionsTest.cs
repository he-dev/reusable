using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.Tests.Utilities.JsonNet.Extensions
{
    [TestClass]
    public class JsonSerializerExtensionsTest
    {
        [TestMethod]
        public void SerializeToBytes_string_bytes()
        {
            var bytes = new JsonSerializer().SerializeToBytes("foo");
            Assert.AreEqual(5, bytes.Length);

            var foo = new JsonSerializer().Deserialize<string>(bytes);
            Assert.AreEqual("foo", foo);
        }
    }
}
