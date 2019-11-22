using Reusable.Data;
using Xunit;

namespace Reusable.Experimental
{
    public class DtoBuilderTest
    {
        [Fact]
        public void Can_create_and_update_object()
        {
            var person =
                DtoUpdater
                    .For<Person>()
                    .With(x => x.FirstName, "Jane")
                    .With(x => x.LastName, null)
                    //.With(x => x.NickName, "JD") // Optional
                    .Commit();

            Assert.Equal("Jane", person.FirstName);
            Assert.Null(person.LastName);
            Assert.Null(person.NickName);

            person =
                person
                    .Update()
                    .With(x => x.LastName, "Doe")
                    .With(x => x.NickName, "JD")
                    .Commit();

            Assert.Equal("Jane", person.FirstName);
            Assert.Equal("Doe", person.LastName);
            Assert.Equal("JD", person.NickName);
        }

        private class Person
        {
            public Person(string firstName, string lastName, string nickName = null)
            {
                FirstName = firstName;
                LastName = lastName;
                NickName = nickName;
            }

            // This ctor should confuse the API.
            public Person(string other) { }

            public string FirstName { get; }

            public string LastName { get; }

            public string NickName { get; set; }

            // This property should confuse the API too.
            public string FullName => $"{LastName}, {FirstName}";
        }
    }

    
}