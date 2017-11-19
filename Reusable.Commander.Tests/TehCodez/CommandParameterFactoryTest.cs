using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.CommandLine;
using Reusable.Exceptionize;
using Reusable.Tester;

namespace Reusable.Commander.Tests
{
    [TestClass]
    public class CommandParameterFactoryTest
    {
        [TestMethod]
        public void CreateCommandParameter_BasicTypes_AllSet()
        {
            var factory = new CommandParameterMapper(CommandParameterMapper.DefaultConverter);
            var testParameter = factory.CreateCommandParameter<TestParameter>(new CommandLine
            {
                {nameof(TestParameter.StringProperty), "abc"},
                {nameof(TestParameter.Int32Property), "123"},
                {nameof(TestParameter.BooleanPropertey), "true"},
                {nameof(TestParameter.Int32ArrayProperty), "4"},
                {nameof(TestParameter.Int32ArrayProperty), "5"},
                {nameof(TestParameter.Int32ArrayProperty), "6"},
                {nameof(TestParameter.DateTimeProperty), "2017/5/1"},
            });

            Assert.IsNotNull(testParameter);
            Assert.AreEqual("abc", testParameter.StringProperty);
            Assert.AreEqual(123, testParameter.Int32Property);
            Assert.AreEqual(true, testParameter.BooleanPropertey);
            CollectionAssert.AreEqual(new[] { 4, 5, 6 }, testParameter.Int32ArrayProperty);
            Assert.AreEqual(new DateTime(2017, 5, 1), testParameter.DateTimeProperty);
        }

        [TestMethod]
        public void CreateCommandParameter_PropertyWithAlias_Set()
        {
            var factory = new CommandParameterMapper(CommandParameterMapper.DefaultConverter);
            var testParameter = factory.CreateCommandParameter<TestParameterWithAlias>(new CommandLine
            {
                {"bar", "abc"},               
            });

            Assert.IsNotNull(testParameter);
            Assert.AreEqual("abc", testParameter.Foo);
        }

        [TestMethod]
        public void CreateCommandParameter_DuplicateNames_Throws()
        {
            var factory = new CommandParameterMapper(CommandParameterMapper.DefaultConverter);
            var ex = Assert.That.ThrowsExceptionFiltered<DynamicException>(() => factory.CreateCommandParameter<TestParameterWithDuplicateNames>(new CommandLine()));
        }        

        private class TestParameter
        {
            public string StringProperty { get; set; }

            public int Int32Property { get; set; }

            [DefaultValue(true)]
            public bool BooleanPropertey { get; set; }

            public int[] Int32ArrayProperty { get; set; }

            public DateTime DateTimeProperty { get; set; }
        }

        private class TestParameterWithAlias
        {
            [Alias("bar")]
            public string Foo { get; set; }
        }

        private class UniqueParameters
        {
            public string Foo { get; set; }

            public string Bar { get; set; }
        }

        private class TestParameterWithDuplicateNames
        {
            public string Foo { get; set; }

            [Alias("foo")]
            public string Bar { get; set; }
        }
    }
}