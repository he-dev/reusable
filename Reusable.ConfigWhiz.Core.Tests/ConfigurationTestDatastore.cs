using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Core.Tests.Data;
using Reusable.ConfigWhiz.Datastores;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Testing;

namespace Reusable.ConfigWhiz.Tests
{
    [TestClass]
    public abstract class ConfigurationTestDatastore
    {
        protected IEnumerable<IDatastore> Datastores { get; set; }
        
        [TestMethod]
        public void Load_ConsumerWithoutName_GotContainer()
        {
            return;

            var memory = new Memory(DatastoreHandle.Default)
            {
                { "Reusable.ConfigWhiz.Tests.Foo.Bar.Baz", "bar" }
            };
            var configuration = new Configuration(ImmutableList<IDatastore>.Empty.Add(memory));

            var foo = configuration.Load<Foo, Bar>();
            foo.Succees.Verify().IsTrue();
            foo.Value.Verify().IsNotNull();
            foo.Value.Baz.Verify().IsEqual("bar");
        }        
    }
}