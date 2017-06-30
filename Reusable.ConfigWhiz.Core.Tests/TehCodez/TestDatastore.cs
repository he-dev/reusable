using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Tests
{
    internal class TestDatastore : Datastore
    {
        public TestDatastore(string name, IEnumerable<Type> supportedTypes) : base(name, supportedTypes)
        {
        }

        protected override ICollection<IEntity> ReadCore(IIdentifier id)
        {
            throw new NotImplementedException();
        }

        protected override int WriteCore(IGrouping<IIdentifier, IEntity> settings)
        {
            throw new NotImplementedException();
        }
    }
}
