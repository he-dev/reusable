using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.ConfigWhiz.Data;
using Reusable.ConfigWhiz.Paths;

namespace Reusable.ConfigWhiz.Tests
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
