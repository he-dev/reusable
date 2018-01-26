using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Reusable.SmartConfig.Tests
{
    [TestClass]
    public class SettingDataStoreTest
    {
        [TestMethod]
        public void MyTestMethod()
        {
            var settingConverter = Mock.Create<ISettingConverter>();
            settingConverter
                .Arrange(x => x.Deserialize(Arg.IsAny<object>(), Arg.IsAny<Type>()))
                .Returns((Func<object, Type, object>)((value, type) => value));

            var dataStore = Mock.Create<SettingDataStore>(Behavior.CallOriginal, settingConverter);
            Mock.NonPublic.Arrange(dataStore, "ReadCore", ArgExpr.IsAny<IEnumerable<SoftString>>()).MustBeCalled();
            
            dataStore.Read("foo", typeof(string));
            dataStore.Assert();
        }
    }
}
