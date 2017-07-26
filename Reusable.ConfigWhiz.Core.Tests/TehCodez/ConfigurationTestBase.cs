using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.Fuse;
using Reusable.Fuse.Drawing;
using Reusable.Fuse.Testing;
using Reusable.SmartConfig.Tests.Common;
using Reusable.SmartConfig.Tests.Common.TestClasses;
using Reusable.TypeConversion;

namespace Reusable.SmartConfig.Tests
{
    //[TestClass]
    public abstract class ConfigurationTestBase
    {
        protected IEnumerable<IDatastore> Datastores { get; set; }

        #region Load tests

        [TestMethod]
        public void IO_Numeric()
        {
            var configuration = new Configuration(Datastores);

            var numeric = new Numeric();

            configuration.Apply(() => numeric.SByte);
            configuration.Apply(() => numeric.Byte);
            //configuration.SetValue(() => numeric.Char);
            configuration.Apply(() => numeric.Int16);
            configuration.Apply(() => numeric.Int32);
            configuration.Apply(() => numeric.Int64);
            configuration.Apply(() => numeric.UInt16);
            configuration.Apply(() => numeric.UInt32);
            configuration.Apply(() => numeric.UInt64);
            configuration.Apply(() => numeric.Single);
            configuration.Apply(() => numeric.Double);
            configuration.Apply(() => numeric.Decimal);

            Assert.AreEqual(SByte.MaxValue, numeric.SByte);
            Assert.AreEqual(Byte.MaxValue, numeric.Byte);
            Assert.AreEqual(Int16.MaxValue, numeric.Int16);
            Assert.AreEqual(Int32.MaxValue, numeric.Int32);
            Assert.AreEqual(Int64.MaxValue, numeric.Int64);
            Assert.AreEqual(UInt16.MaxValue, numeric.UInt16);
            Assert.AreEqual(UInt32.MaxValue, numeric.UInt32);
            Assert.AreEqual(UInt64.MaxValue, numeric.UInt64);
            Assert.AreEqual(Single.MaxValue, numeric.Single);
            Assert.AreEqual(Double.MaxValue, numeric.Double);
            Assert.AreEqual(Decimal.MaxValue, numeric.Decimal);

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

            configuration.Update(() => numeric.SByte);
            configuration.Update(() => numeric.Byte);
            configuration.Update(() => numeric.Int16);
            configuration.Update(() => numeric.Int32);
            configuration.Update(() => numeric.Int64);
            configuration.Update(() => numeric.UInt16);
            configuration.Update(() => numeric.UInt32);
            configuration.Update(() => numeric.UInt64);
            configuration.Update(() => numeric.Single);
            configuration.Update(() => numeric.Double);
            configuration.Update(() => numeric.Decimal);

            configuration = new Configuration(Datastores);

            numeric = new Numeric();

            configuration.Apply(() => numeric.SByte);
            configuration.Apply(() => numeric.Byte);
            //configuration.SetValue(() => numeric.Char);
            configuration.Apply(() => numeric.Int16);
            configuration.Apply(() => numeric.Int32);
            configuration.Apply(() => numeric.Int64);
            configuration.Apply(() => numeric.UInt16);
            configuration.Apply(() => numeric.UInt32);
            configuration.Apply(() => numeric.UInt64);
            configuration.Apply(() => numeric.Single);
            configuration.Apply(() => numeric.Double);
            configuration.Apply(() => numeric.Decimal);

            Assert.AreEqual(SByte.MaxValue - 1, numeric.SByte);
            Assert.AreEqual(Byte.MaxValue - 1, numeric.Byte);
            Assert.AreEqual(Int16.MaxValue - 1, numeric.Int16);
            Assert.AreEqual(Int32.MaxValue - 1, numeric.Int32);
            Assert.AreEqual(Int64.MaxValue - 1, numeric.Int64);
            Assert.AreEqual(UInt16.MaxValue - 1, numeric.UInt16);
            Assert.AreEqual(UInt32.MaxValue - 1, numeric.UInt32);
            Assert.AreEqual(UInt64.MaxValue - 1, numeric.UInt64);
            Assert.AreEqual(Single.MinValue, numeric.Single);
            Assert.AreEqual(Double.MinValue, numeric.Double);
            Assert.AreEqual(Decimal.MaxValue - 1, numeric.Decimal);            
        }

        [TestMethod]
        public void IO_Literal()
        {
            var config = new Configuration(Datastores);

            var literal = new Literal();// configuration.Get<LiteralConfiguration>();

            config.Apply(() => literal.StringPL);
            config.Apply(() => literal.StringDE);

            Assert.AreEqual("ąęśćżźó", literal.StringPL);
            Assert.AreEqual("äöüß", literal.StringDE);

            literal.StringDE += "---";
            literal.StringPL += "---";

            config.Update(() => literal.StringPL);
            config.Update(() => literal.StringDE);

            config = new Configuration(Datastores);
            literal = new Literal();

            config.Apply(() => literal.StringPL);
            config.Apply(() => literal.StringDE);

            Assert.AreEqual("ąęśćżźó---", literal.StringPL);
            Assert.AreEqual("äöüß---", literal.StringDE);
        }

        [TestMethod]
        public void IO_Other()
        {
            var config = new Configuration(Datastores);

            var other = new Other();

            config.Apply(() => other.Boolean);
            config.Apply(() => other.Enum);
            config.Apply(() => other.DateTime);
            
            Assert.AreEqual(true, other.Boolean);
            Assert.AreEqual(TestEnum.TestValue2, other.Enum);
            Assert.AreEqual(new DateTime(2016, 7, 30), other.DateTime);

            other.Boolean = !other.Boolean;
            other.Enum = TestEnum.TestValue3;
            other.DateTime = other.DateTime.AddDays(-1);

            config.Update(() => other.Boolean);
            config.Update(() => other.Enum);
            config.Update(() => other.DateTime);

            config = new Configuration(Datastores);

            other = new Other();

            config.Apply(() => other.Boolean);
            config.Apply(() => other.Enum);
            config.Apply(() => other.DateTime);

            Assert.AreEqual(false, other.Boolean);
            Assert.AreEqual(TestEnum.TestValue3, other.Enum);
            Assert.AreEqual(new DateTime(2016, 7, 30).AddDays(-1), other.DateTime);
        }

        [TestMethod]
        public void IO_Painting()
        {
            var config = new Configuration(Datastores);

            var paint = new Painting();

            config.Apply(() => paint.ColorName);
            config.Apply(() => paint.ColorDec);
            config.Apply(() => paint.ColorHex);

            Assert.AreEqual(Color.Red, paint.ColorName);
            Assert.AreEqual(Color.Plum, paint.ColorDec);
            Assert.AreEqual(Color.Beige, paint.ColorHex);

            // modify


        }

        //[TestMethod]
        //public void Load_Collection_Loaded()
        //{
        //    var configuration = Configuration.Builder
        //        .WithDatastores(Datastores)
        //        .WithConverter<JsonToObjectConverter<List<int>>>()
        //        .WithConverter<ObjectToJsonConverter<List<int>>>()
        //        .Build();

        //    var collection = configuration.Get<CollectionConfiguration>();
        //    collection.Verify().IsNotNull();
        //    collection.JsonArray.Verify().SequenceEqual(new[] { 5, 8, 13 });
        //    collection.ArrayInt32.Verify().SequenceEqual(new[] { 5, 8 });
        //    collection.DictionaryStringInt32.Verify().IsNotNull();
        //    collection.DictionaryStringInt32.Verify().DictionaryEqual(new Dictionary<string, int> { ["foo"] = 21, ["bar"] = 34 });

        //    // modify

        //    collection.JsonArray = new List<int>(new[] { 8, 9, 0, 1 });
        //    collection.ArrayInt32 = new[] { 4, 6 };
        //    collection.DictionaryStringInt32.Add("baz", 88);

        //    // reaload

        //    configuration.Save();
        //    configuration = Configuration.Builder
        //        .WithDatastores(Datastores)
        //        .WithConverter<JsonToObjectConverter<List<int>>>()
        //        .WithConverter<ObjectToJsonConverter<List<int>>>()
        //        .Build();
        //    collection = configuration.Get<CollectionConfiguration>();

        //    // verify

        //    collection.JsonArray.Verify().SequenceEqual(new[] { 8, 9, 0, 1 });
        //    collection.ArrayInt32.Verify().SequenceEqual(new[] { 4, 6 });
        //    collection.DictionaryStringInt32.Verify().IsNotNull();
        //    collection.DictionaryStringInt32.Verify().DictionaryEqual(new Dictionary<string, int> { ["foo"] = 21, ["bar"] = 34, ["baz"] = 88 });
        //}

        #endregion
    }
}