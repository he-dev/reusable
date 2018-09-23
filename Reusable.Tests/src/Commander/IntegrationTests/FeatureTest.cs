using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Utilities.MSTest;

namespace Reusable.Tests.Commander.IntegrationTests
{
    [TestClass]
    public class FeatureTest : IntegrationTest
    {
        [TestMethod]
        public async Task ExecuteAsync_SingleCommand_Executed()
        {
            await Executor.ExecuteAsync("cmd-a");

            ExecuteAssert<Bag1>(bag =>
            {
                Assert.IsFalse(bag.Bool1);
//                Assert.IsTrue(bag.Property02);
//                Assert.IsTrue(bag.Property03);
//                Assert.AreEqual("bar", bag.Property04);
//                Assert.That.Collection().AreEqual(new[] {1, 2, 3}, bag.Property05);
            });
        }
        
        [TestMethod]
        public async Task ExecuteAsync_MultipleCommands_Executed()
        {
            await Executor.ExecuteAsync("cmd1 -p03:true -p04:bar -p05 1 2 3 | cmd2 -p04:baz");

            ExecuteAssert<Bag1>(bag =>
            {
                Assert.IsFalse(bag.Bool1);
                Assert.IsTrue(bag.Bool2);
                Assert.IsTrue(bag.Bool3);
                Assert.AreEqual("bar", bag.String1);
                Assert.That.Collection().AreEqual(new[] {1, 2, 3}, bag.List1);
            });

            ExecuteAssert<Bag2>(bag => { Assert.AreEqual("baz", bag.Property04); });
        }
    }
}