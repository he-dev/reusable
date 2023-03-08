using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Utilities.JsonNet.Annotations;
using Reusable.Utilities.JsonNet.Converters;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Reusable.Utilities.JsonNet
{
    public class JsonStringConverterTest
    {
        [Fact]
        public void Uses_ctor_Parse_or_custom()
        {
            var expected = @"{""UserWithCtor"":""Tom"",""UserWithParse"":""Bob"",""Person"":""Sam""}";
            var test = JsonConvert.DeserializeObject<Test>(expected, new JsonStringConverter(typeof(Person)));

            Assert.NotNull(test);
            Assert.Equal("Sam", test.Person.Name);
            Assert.Equal("Tom", test.UserWithCtor.Name);
            Assert.Equal("Bob", test.UserWithParse.Name);

            var actual = JsonConvert.SerializeObject(test, Formatting.None, new JsonStringConverter(typeof(Person)));

            Assert.Equal(expected, actual);
        }        

        [PublicAPI]
        [JsonString]
        private abstract class User
        {
            public string Name { get; set; }

            public override string ToString() => Name;
        }

        [UsedImplicitly]
        private class UserWithCtor : User
        {
            public UserWithCtor(string name) => Name = name;
        }

        [UsedImplicitly]
        private class UserWithParse : User
        {
            public static UserWithParse Parse(string name) => new UserWithParse { Name = name };
        }
        
        [PublicAPI]
        private class Person
        {
            public Person(string name) => Name = name;
            
            public string Name { get; set; }

            public override string ToString() => Name;
        }

        [PublicAPI]
        [UsedImplicitly]
        private class Test
        {
            public UserWithCtor UserWithCtor { get; set; }

            public UserWithParse UserWithParse { get; set; }
            
            public Person Person { get; set; }
        }
    }
}