using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.SmartConfig.Tests
{
    [TestClass]
    public class SettingConverterTest
    {
        [TestMethod]
        public void Deserialize_ValueHasTargetType_DeserializeCoreNotCalled()
        {
            var settingConverter = Mock.Create<SettingConverter>(Behavior.CallOriginal, (IEnumerable<Type>)new[] { typeof(string) });

            Mock
                .NonPublic
                .Arrange<object>(settingConverter, "DeserializeCore", ArgExpr.IsAny<object>(), ArgExpr.IsAny<Type>())
                .OccursNever();

            var result = settingConverter.Deserialize("foo", typeof(string));

            settingConverter.Assert();
            Assert.AreEqual("foo", result);
        }

        [TestMethod]
        public void Deserialize_ValueHasOtherType_DeserializeCoreCalled()
        {
            var settingConverter = Mock.Create<SettingConverter>(Behavior.CallOriginal, (IEnumerable<Type>)new[] { typeof(string) });

            Mock
                .NonPublic
                .Arrange<object>(settingConverter, "DeserializeCore", ArgExpr.IsAny<object>(), ArgExpr.IsAny<Type>())
                .OccursOnce();

            settingConverter.Deserialize("foo", typeof(int));

            settingConverter.Assert();
        }

        [TestMethod]
        public void Serialize_ValueHasSupportedType_SerializeCoreCalledWithSupportedType()
        {
            var settingConverter = Mock.Create<SettingConverter>(Behavior.CallOriginal, (IEnumerable<Type>)new[] { typeof(string) });

            Mock
                .NonPublic
                .Arrange<object>(
                    settingConverter,
                    "SerializeCore",
                    ArgExpr.IsAny<object>(),
                    ArgExpr.Matches<Type>(t => t == typeof(string)))
                .OccursOnce();

            settingConverter.Serialize("foo");

            settingConverter.Assert();
        }

        [TestMethod]
        public void Serialize_ValueHasUnsupportedType_SerializeCoreCalledWithFallbackType()
        {
            var settingConverter = Mock.Create<SettingConverter>(Behavior.CallOriginal, (IEnumerable<Type>)new[] { typeof(string) });

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
