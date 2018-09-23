using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Commander.IntegrationTests
{
    [TestClass]
    public class FeatureTest : IntegrationTest
    {
        [TestMethod]
        public async Task ExecuteAsync()
        {
            //Assert.That.Throws<DynamicException>(
            //    () =>
            //    {
            //        executor.ExecuteAsync("cmd3").GetAwaiter().GetResult();
            //    },
            //    filter => filter.WhenName("CommandNotFoundException")
            //);               

            await Executor.ExecuteAsync("cmd1 -p03:true -p04:bar -p05 1 2 3 | cmd2 -p04:baz");

            ExecuteAssert<Bag1>(bag =>
            {
                Assert.IsFalse(bag.Property01);
                Assert.IsTrue(bag.Property02);
                Assert.IsTrue(bag.Property03);
                Assert.AreEqual("bar", bag.Property04);
                Assert.That.Collection().AreEqual(new[] {1, 2, 3}, bag.Property05);
            });

            ExecuteAssert<Bag2>(bag => { Assert.AreEqual("baz", bag.Property04); });
        }
    }
}