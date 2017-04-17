using System;
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
        public void Load_Numeric_Success()
        {
            var configuration = new Configuration(Datastores);

            var load = configuration.Load<Foo, Numeric>();
            load.Succees.Verify().IsTrue();
            load.Value.Verify().IsNotNull();

            var numeric = load.Value;
            numeric.SByte.Verify().IsEqual(SByte.MaxValue);
            numeric.Byte.Verify().IsEqual(Byte.MaxValue);
            numeric.Char.Verify().IsEqual(Char.MaxValue);
            numeric.Int16.Verify().IsEqual(Int16.MaxValue);
            numeric.Int32.Verify().IsEqual(Int32.MaxValue);
            numeric.Int64.Verify().IsEqual(Int64.MaxValue);
            numeric.UInt16.Verify().IsEqual(UInt16.MaxValue);
            numeric.UInt32.Verify().IsEqual(UInt32.MaxValue);
            numeric.UInt64.Verify().IsEqual(UInt64.MaxValue);
            numeric.Single.Verify().IsEqual(Single.MaxValue);
            numeric.Double.Verify().IsEqual(Double.MaxValue);
            numeric.Decimal.Verify().IsEqual(Decimal.MaxValue);
        }
    }
}