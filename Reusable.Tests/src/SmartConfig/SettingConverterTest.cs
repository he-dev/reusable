using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.Tests.SmartConfig
{
    [TestClass]
    public class SettingConverterTest
    {
        
        [TestMethod]
        public void DoesNotCallDeserializeCoreWhenSameTypes()
        {
            var settingConverter = Mock.Create<SettingConverter>(Behavior.CallOriginal, (IEnumerable<Type>)new[] { typeof(string) }, typeof(string));

            Mock
                .NonPublic
                .Arrange<object>(
                    settingConverter, 
                    "DeserializeCore", 
                    ArgExpr.IsAny<object>(), 
                    ArgExpr.IsAny<Type>())
                .OccursNever();

            var result = settingConverter.Deserialize("foo", typeof(string));

            settingConverter.Assert();
            Assert.AreEqual("foo", result);
        }

        [TestMethod]
        public void CallsDeserializeCoreWhenDifferentTypes()
        {
            var settingConverter = Mock.Create<SettingConverter>(Behavior.CallOriginal, (IEnumerable<Type>)new[] { typeof(string) }, typeof(string));

            Mock
                .NonPublic
                .Arrange<object>(
                    settingConverter, 
                    "DeserializeCore", 
                    ArgExpr.IsAny<object>(), 
                    ArgExpr.IsAny<Type>())
                .OccursOnce();

            settingConverter.Deserialize("foo", typeof(int));

            settingConverter.Assert();
        }

        [TestMethod]
        public void DoesNotCallSerializeCoreWhenSameTypes()
        {
            var settingConverter = Mock.Create<SettingConverter>(Behavior.CallOriginal, (IEnumerable<Type>)new[] { typeof(string) }, typeof(string));

            Mock
                .NonPublic
                .Arrange<object>(
                    settingConverter,
                    "SerializeCore",
                    ArgExpr.IsAny<object>(),
                    ArgExpr.IsAny<Type>())
                .OccursNever();

            settingConverter.Serialize("foo");

            settingConverter.Assert();
        }

        [TestMethod]
        public void CallsSerializeCoreWithFallbackTypeWhenTypeNotSupported()
        {
            var settingConverter = Mock.Create<SettingConverter>(Behavior.CallOriginal, (IEnumerable<Type>)new[] { typeof(string) }, typeof(string));

            Mock
                .NonPublic
                .Arrange<object>(
                    settingConverter,
                    "SerializeCore",
                    ArgExpr.IsAny<object>(),
                    ArgExpr.Matches<Type>(t => t == typeof(string)))
                .OccursOnce();

            settingConverter.Serialize(123);

            settingConverter.Assert();
        }
    }
}
