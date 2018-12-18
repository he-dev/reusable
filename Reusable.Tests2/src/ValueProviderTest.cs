using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;
using Xunit;

namespace Reusable.Tests2
{
    public class ValueProviderTest
    {
        [Fact]
        public async Task MyTestMethod()
        {
            var appSettings = new AppSettingProvider();
            //var salute = await appSettings.GetAsync("abc:Salute");
            var salute = await appSettings.GetAsync("setting:salute?prefix=abc");

            Assert.True(salute.Exists);

            var sqlServer = new SqlServerProvider("name=TestDb", ResourceMetadata.Empty);
            //var greeting = await sqlServer.GetAsync("Greeting");

            //Assert.True(greeting.Exists);

            var jsonProvider = sqlServer.DecorateWith(JsonResourceProvider.Factory());

            var car = new Car();
            car.Name = await jsonProvider.GetSettingAsync(() => car.Name);

            //var carInfo = await jsonProvider.GetValueInfoAsync("Car");

            //Assert.True(carInfo.Exists);

            //var car = await carInfo.DeserializeAsync<Car>();

            Assert.Equal("VW", car.Name);

        }

        private class Car
        {
            [SettingMember(Strength = SettingNameStrength.Medium)]
            public string Name { get; set; }
        }
    }

    public class SimpleUriTest
    {
        [Fact]
        public void Test1()
        {
            var a = new UriString("file:c:/temp");
            var r = new UriString("/logs/test.log");
            var u = a + r;
            Assert.Equal(new UriString("file:c:/temp/logs/test.log"), u);
        }
    }
}
