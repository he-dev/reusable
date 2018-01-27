using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig.Data;
using Reusable.Tester;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.SmartConfig.DataStores.Tests
{
    [TestClass]
    public class InMemoryTest
    {
        [TestMethod]
        public void Add_Settings_Added()
        {
            var converter = Mock.Create<ISettingConverter>();
            Mock
                //.Arrange(() => converter.Serialize(Arg.IsAny<object>(), Arg.IsAny<Type>()))
                .Arrange(() => converter.Serialize(Arg.IsAny<object>()))
                .Returns(obj => obj)
                .Occurs(2);

            var inMemory = new InMemory(converter)
            {
                {"foo", "fooo"},
                {"bar", "baar"},
            };

            converter.Assert();
            Assert.That.Collection().CountEquals(2, inMemory);
        }

        [TestMethod]
        public void Write_Values_Written()
        {
            var converter = Mock.Create<ISettingConverter>();

            Mock
                .Arrange(() => converter.Deserialize(Arg.IsAny<object>(), Arg.IsAny<Type>()))
                //.Arrange(() => converter.Serialize(Arg.IsAny<object>()))
                .Returns(obj => obj);
                //.Occurs(2);


            Mock
                .Arrange(() => converter.Serialize(Arg.IsAny<object>()))
                .Returns(obj => obj)
                .Occurs(5);

            var inMemory = new InMemory(converter)
            {
                {"foo", "fooo"},
                {"bar", "baar"},
            };

            inMemory.Write(new Setting("foo") { Value = "fooo" });
            inMemory.Write(new Setting("bar") { Value = "baaar" });
            inMemory.Write(new Setting("baz") { Value = "baz" });

            converter.Assert();
            Assert.That.Collection().CountEquals(3, inMemory);
        }        
    }
}
