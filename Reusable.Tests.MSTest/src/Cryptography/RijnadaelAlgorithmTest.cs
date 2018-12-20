using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Cryptography;
using Reusable.Cryptography.Extensions;

namespace Reusable.Tests.Cryptography
{
    [TestClass]
    public class RijnadaelAlgorithmTest
    {
        [TestMethod]
        public async Task MyTestMethod()
        {
            var rgbIV = RijndaelAlgorithm.CreateRgbIV();
            var algorithm = new RijndaelAlgorithm("12345678".ToBytes(), rgbIV);

            var foo = "foo".ToBytes();

            var encrypted = await algorithm.EncryptAsync(foo, "bar");
            var decrypted = await algorithm.DecryptAsync(encrypted, "bar");

            Assert.AreEqual("foo", decrypted.GetString());
        }
    }
}
