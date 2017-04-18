using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.ConfigWhiz.Core.Tests.Data;
using Reusable.ConfigWhiz.Datastores;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Drawing;
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

        [TestMethod]
        public void Load_Literal_Success()
        {
            var configuration = new Configuration(Datastores);

            var load = configuration.Load<Foo, Literal>();
            load.Succees.Verify().IsTrue();
            load.Value.Verify().IsNotNull();

            var literal = load.Value;
            literal.StringDE.Verify().IsEqual("äöüß");
            literal.StringPL.Verify().IsEqual("ąęśćżźó");
        }

        [TestMethod]
        public void Load_Other_Success()
        {
            var configuration = new Configuration(Datastores);

            var load = configuration.Load<Foo, Other>();
            load.Succees.Verify().IsTrue();
            load.Value.Verify().IsNotNull();

            var other = load.Value;
            other.Boolean.Verify().IsTrue();
            other.Enum.Verify().IsEqual(TestEnum.TestValue2);
            other.DateTime.Verify().IsEqual(new DateTime(2016, 7, 30));
        }

        [TestMethod]
        public void Load_Paint_Success()
        {
            var configuration = new Configuration(Datastores);

            var load = configuration.Load<Foo, Paint>();
            load.Succees.Verify().IsTrue();
            load.Value.Verify().IsNotNull();

            var paint = load.Value;
            paint.ColorName.Verify().IsEqual(Color.DarkRed);
            paint.ColorDec.Verify().IsEqual(Color.Plum);
            paint.ColorHex.Verify().IsEqual(Color.Beige);
        }

        [TestMethod]
        public void Load_Collection_Success()
        {
            var configuration = new Configuration(Datastores);

            var load = configuration.Load<Foo, Collection>();
            load.Succees.Verify().IsTrue();
            load.Value.Verify().IsNotNull();

            var collection = load.Value;
            collection.JsonArray.Verify().SequenceEqual(new[] { 5, 8, 13 });
            collection.ArrayInt32.Verify().SequenceEqual(new[] { 5, 8 });
            collection.DictionaryStringInt32.Verify().IsNotNull();
            collection.DictionaryStringInt32.Verify().DictionaryEqual(new Dictionary<string, int> { ["foo"] = 21, ["bar"] = 34 });
        }
    }
}