using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Commander;

namespace Reusable.Tests.Commander.Integration
{
    using static Helper;

    [TestClass]
    public class FeatureIntegrationTest
    {
        [TestMethod]
        public async Task CanExecuteCommandByAnyName()
        {
            var executeCount = 0;

            using (var context = CreateContext(
                commands => commands
                    .Add(Identifier.Create("a", "b"), Execute<SimpleBag>((name, bag, ct) => { executeCount++; }))
            ))
            {
                await context.Executor.ExecuteAsync("a");
                await context.Executor.ExecuteAsync("b");

                Assert.AreEqual(2, executeCount);
            }
        }

        [TestMethod]
        public async Task CanExecuteMultipleCommands()
        {
            var executeCount = 0;

            using (var context = CreateContext(
                commands => commands
                    .Add(Identifier.Create("a"), Execute<SimpleBag>((i, b, c) => executeCount++))
                    .Add(Identifier.Create("b"), Execute<SimpleBag>((i, b, c) => executeCount++))
                ))
            {
                await context.Executor.ExecuteAsync("a|b");

                Assert.AreEqual(2, executeCount);
            }
        }

        [TestMethod]
        public async Task CanMapParameterByName()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", Track<BagWithoutAliases>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync("c -StringWithoutAlias=abc");
                tracker.Assert<BagWithoutAliases>(
                    "c",
                    bag =>
                    {
                        Assert.IsFalse(bag.Async);
                        Assert.IsFalse(bag.CanThrow);
                        Assert.AreEqual("abc", bag.StringWithoutAlias);
                    }
                );
            }
        }

        [TestMethod]
        public async Task CanMapParameterByAlias()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", Track<BagWithAliases>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync("c -swa=abc");
                tracker.Assert<BagWithAliases>(
                    "c",
                    bag =>
                    {
                        Assert.IsFalse(bag.Async);
                        Assert.IsFalse(bag.CanThrow);
                        Assert.AreEqual("abc", bag.StringWithAlias);
                    }
                );
            }
        }

        [TestMethod]
        public async Task CanCreateBagWithDefaultValues()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", Track<BagWithDefaultValues>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync("c");
                tracker.Assert<BagWithDefaultValues>(
                    "c",
                    bag =>
                    {
                        Assert.IsFalse(bag.Async);
                        Assert.IsFalse(bag.CanThrow);
                        Assert.IsFalse(bag.BoolOnly);
                        Assert.IsFalse(bag.BoolWithDefaultValue1);
                        Assert.IsTrue(bag.BoolWithDefaultValue2);
                        Assert.IsNull(bag.StringOnly);
                        Assert.AreEqual("foo", bag.StringWithDefaultValue);
                        Assert.AreEqual(0, bag.Int32Only);
                        Assert.IsNull(bag.NullableInt32Only);
                        Assert.AreEqual(3, bag.Int32WithDefaultValue);
                        Assert.AreEqual(DateTime.MinValue, bag.DateTimeOnly);
                        Assert.IsNull(bag.NullableDateTime);
                        Assert.AreEqual(new DateTime(2018, 1, 1), bag.DateTimeWithDefaultValue);
                        Assert.IsNull(bag.ListOnly);
                    }
                );
            }
        }

        [TestMethod]
        public async Task CanCreateBagWithFlagValues()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", Track<SimpleBag>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync("c -async -canthrow=true");
                tracker.Assert<SimpleBag>(
                    "c",
                    bag =>
                    {
                        Assert.IsTrue(bag.Async);
                        Assert.IsTrue(bag.CanThrow);
                    }
                );
            }
        }

        [TestMethod]
        public async Task CanCreateBagWithCommandLineValues()
        {
            var tracker = new BagTracker();
            using (var context = CreateContext(
                commands => commands
                    .Add("c", Track<BagWithMappedValues>(tracker))
            ))
            {
                await context.Executor.ExecuteAsync("c");
                tracker.Assert<BagWithMappedValues>(
                    "c",
                    bag => { }
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