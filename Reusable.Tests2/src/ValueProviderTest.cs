using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Annotations;
using Reusable.Stratus;
using Xunit;

namespace Reusable.Tests2
{
    public class ValueProviderTest
    {
        [Fact]
        public async Task MyTestMethod()
        {
            var appSettings = new AppSettingProvider();
            var salute = await appSettings.GetValueInfoAsync("abc:Salute");

            Assert.True(salute.Exists);

            var sqlServer = new SqlServerProvider("name=TestDb", ValueProviderMetadata.Empty);
            var greeting = await sqlServer.GetValueInfoAsync("Greeting");

            Assert.True(greeting.Exists);

            var jsonProvider = sqlServer.DecorateWith(JsonValueProvider.Factory());

            var car = new Car();
            car.Name = await jsonProvider.GetAsync(() => car.Name);

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
}
