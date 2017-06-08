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
    //[TestClass]
    public abstract class ConfigurationTestDatastore
    {
        protected IEnumerable<IDatastore> Datastores { get; set; }

        #region Load tests

        [TestMethod]
        public void Load_Numeric_Success()
        {
            var configuration = new Configuration(Datastores);

            var numeric = configuration.Load<Foo, Numeric>();
            numeric.Verify().IsNotNull();

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

            // Modify settings

            numeric.SByte -= 1;
            numeric.Byte -= 1;
            numeric.Char = (char)(Char.MaxValue - 1);
            numeric.Int16 -= 1;
            numeric.Int32 -= 1;
            numeric.Int64 -= 1;
            numeric.UInt16 -= 1;
            numeric.UInt32 -= 1;
            numeric.UInt64 -= 1;
            numeric.Single = Single.MinValue;
            numeric.Double = Double.MinValue;
            numeric.Decimal -= 1;

            configuration.Save();

            configuration = new Configuration(Datastores);
            numeric = configuration.Load<Foo, Numeric>();

            numeric.SByte.Verify().IsEqual((SByte)(SByte.MaxValue - 1));
            numeric.Byte.Verify().IsEqual((Byte)(Byte.MaxValue - 1));
            numeric.Char.Verify().IsEqual((Char)(Char.MaxValue - 1));
            numeric.Int16.Verify().IsEqual((Int16)(Int16.MaxValue - 1));
            numeric.Int32.Verify().IsEqual(Int32.MaxValue - 1);
            numeric.Int64.Verify().IsEqual(Int64.MaxValue - 1);
            numeric.UInt16.Verify().IsEqual((UInt16)(UInt16.MaxValue - 1));
            numeric.UInt32.Verify().IsEqual(UInt32.MaxValue - 1);
            numeric.UInt64.Verify().IsEqual(UInt64.MaxValue - 1);
            numeric.Single.Verify().IsEqual(Single.MinValue);
            numeric.Double.Verify().IsEqual(Double.MinValue);
            numeric.Decimal.Verify().IsEqual(Decimal.MaxValue - 1);
        }

        [TestMethod]
        public void Load_Literal_Success()
        {
            var configuration = new Configuration(Datastores);

            var literal = configuration.Load<Foo, Literal>();
            literal.Verify().IsNotNull();
            literal.StringDE.Verify().IsEqual("äöüß");
            literal.StringPL.Verify().IsEqual("ąęśćżźó");

            literal.StringDE += "---";
            literal.StringPL += "---";

            configuration.Save();

            configuration = new Configuration(Datastores);
            literal = configuration.Load<Foo, Literal>();

            literal.StringDE.Verify().IsEqual("äöüß---");
            literal.StringPL.Verify().IsEqual("ąęśćżźó---");
        }

        [TestMethod]
        public void Load_Other_Success()
        {
            var configuration = new Configuration(Datastores);

            var other = configuration.Load<Foo, Other>();
            other.Verify().IsNotNull();
            other.Boolean.Verify().IsTrue();
            other.Enum.Verify().IsEqual(TestEnum.TestValue2);
            other.DateTime.Verify().IsEqual(new DateTime(2016, 7, 30));


            other.Boolean = !other.Boolean;
            other.Enum = TestEnum.TestValue3;
            other.DateTime = other.DateTime.AddDays(-1);

            configuration.Save();
            configuration = new Configuration(Datastores);
            other = configuration.Load<Foo, Other>();

            other.Boolean.Verify().IsFalse();
            other.Enum.Verify().IsTrue(x => x == TestEnum.TestValue3);
            other.DateTime.Verify().IsEqual(new DateTime(2016, 7, 30).AddDays(-1));
        }

        [TestMethod]
        public void Load_Paint_Success()
        {
            var configuration = new Configuration(Datastores);

            var paint = configuration.Load<Foo, Paint>();
            paint.Verify().IsNotNull();
            paint.ColorName.Verify().IsEqual(Color.DarkRed);
            paint.ColorDec.Verify().IsEqual(Color.Plum);
            paint.ColorHex.Verify().IsEqual(Color.Beige);

            // modify


        }

        [TestMethod]
        public void Load_Collection_Success()
        {
            var configuration = new Configuration(Datastores);

            var collection = configuration.Load<Foo, Collection>();
            collection.Verify().IsNotNull();
            collection.JsonArray.Verify().SequenceEqual(new[] { 5, 8, 13 });
            collection.ArrayInt32.Verify().SequenceEqual(new[] { 5, 8 });
            collection.DictionaryStringInt32.Verify().IsNotNull();
            collection.DictionaryStringInt32.Verify().DictionaryEqual(new Dictionary<string, int> { ["foo"] = 21, ["bar"] = 34 });

            // modify

            collection.JsonArray = new List<int>(new[] { 8, 9, 0, 1 });
            collection.ArrayInt32 = new[] { 4, 6 };
            collection.DictionaryStringInt32.Add("baz", 88);

            // reaload

            configuration.Save();
            configuration = new Configuration(Datastores);
            collection = configuration.Load<Foo, Collection>();

            // verify

            collection.JsonArray.Verify().SequenceEqual(new[] { 8, 9, 0, 1 });
            collection.ArrayInt32.Verify().SequenceEqual(new[] { 4, 6 });
            collection.DictionaryStringInt32.Verify().IsNotNull();
            collection.DictionaryStringInt32.Verify().DictionaryEqual(new Dictionary<string, int> { ["foo"] = 21, ["bar"] = 34, ["baz"] = 88 });
        }

        [TestMethod]
        public void Load_DataSource_Cache_SameObject()
        {
            var configuration = new Configuration(Datastores);

            var numeric1 = configuration.Load<Foo, Numeric>();
            var numeric2 = configuration.Load<Foo, Numeric>();
            Assert.IsNotNull(numeric1);
            Assert.IsNotNull(numeric2);
            Assert.AreSame(numeric1, numeric2);
        }

        [TestMethod]
        public void Load_DataSource_Provider_SameObject()
        {
            var configuration = new Configuration(Datastores);

            var numeric1 = configuration.Load<Foo, Numeric>();
            var numeric2 = configuration.Load<Foo, Numeric>(DataSource.Provider);
            Assert.IsNotNull(numeric1);
            Assert.IsNotNull(numeric2);
            Assert.AreSame(numeric1, numeric2);
        }

        #endregion
    }
}