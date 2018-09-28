using System;
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
            var bags = new BagTracker();
            using (CreateContext(
                commands => commands
                    .Add("c", CreateExecuteCallback<Bag1>(bags)), out var executor
            ))
            {
                await executor.ExecuteAsync("c");
                bags.Assert<Bag1>(
                    "c",
                    bag =>
                    {
                        Assert.IsFalse(bag.Bool1);
                        Assert.IsFalse(bag.Bool2);
                        Assert.IsTrue(bag.Bool3);
                        Assert.IsNull(bag.String1);
                        Assert.AreEqual("foo", bag.String2);
                        Assert.AreEqual(0, bag.Int1);
                        Assert.IsNull(bag.Int2);
                        Assert.AreEqual(3, bag.Int3);
                        Assert.AreEqual(DateTime.MinValue, bag.DateTime1);
                        Assert.IsNull(bag.DateTime2);
                        Assert.AreEqual(new DateTime(2018, 1, 1), bag.DateTime3);
                        Assert.IsNull(bag.List1);
                    }
                );
            }
        }

        // [TestMethod]
        // public async Task ExecuteAsync_MultipleCommands_Executed()
        // {
        //     await Executor.ExecuteAsync("cmd1 -p03:true -p04:bar -p05 1 2 3 | cmd2 -p04:baz");
        //     
        //     ExecuteAssert<Bag1>(
        //         bag =>
        //         {
        //             Assert.IsFalse(bag.Bool1);
        //             Assert.IsTrue(bag.Bool2);
        //             Assert.IsTrue(bag.Bool3);
        //             Assert.AreEqual("bar", bag.String1);
        //             Assert.That.Collection().AreEqual(new[] { 1, 2, 3 }, bag.List1);
        //         }
        //     );
        //     
        //     ExecuteAssert<Bag2>(bag => { Assert.AreEqual("baz", bag.Property04); });
        // }
    }
}