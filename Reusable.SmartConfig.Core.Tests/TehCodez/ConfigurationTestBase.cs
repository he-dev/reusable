using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.SmartConfig.Binding;
using Reusable.SmartConfig.Tests.Common;
using Reusable.SmartConfig.Tests.Common.TestClasses;

namespace Reusable.SmartConfig.Tests
{
    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void Select_()
        {
            //var converter = 
        }
    }

    //[TestClass]
    public abstract class ConfigurationTestBase
    {
        protected IEnumerable<ISettingDataStore> Datastores { get; set; }

        #region Load tests

        [TestMethod]
        public void IO_Numeric()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });

            var numeric = new Numeric();

            config.Bind(() => numeric.SByte);
            config.Bind(() => numeric.Byte);
            //configuration.SetValue(() => numeric.Char);
            config.Bind(() => numeric.Int16);
            config.Bind(() => numeric.Int32);
            config.Bind(() => numeric.Int64);
            config.Bind(() => numeric.UInt16);
            config.Bind(() => numeric.UInt32);
            config.Bind(() => numeric.UInt64);
            config.Bind(() => numeric.Single);
            config.Bind(() => numeric.Double);
            config.Bind(() => numeric.Decimal);

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

            config.Update(() => numeric.SByte);
            config.Update(() => numeric.Byte);
            config.Update(() => numeric.Int16);
            config.Update(() => numeric.Int32);
            config.Update(() => numeric.Int64);
            config.Update(() => numeric.UInt16);
            config.Update(() => numeric.UInt32);
            config.Update(() => numeric.UInt64);
            config.Update(() => numeric.Single);
            config.Update(() => numeric.Double);
            config.Update(() => numeric.Decimal);

            config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });

            numeric = new Numeric();

            config.Bind(() => numeric.SByte);
            config.Bind(() => numeric.Byte);
            //configuration.SetValue(() => numeric.Char);
            config.Bind(() => numeric.Int16);
            config.Bind(() => numeric.Int32);
            config.Bind(() => numeric.Int64);
            config.Bind(() => numeric.UInt16);
            config.Bind(() => numeric.UInt32);
            config.Bind(() => numeric.UInt64);
            config.Bind(() => numeric.Single);
            config.Bind(() => numeric.Double);
            config.Bind(() => numeric.Decimal);

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
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });

            var literal = new Literal();// configuration.Get<LiteralConfiguration>();

            config.Bind(() => literal.StringPL);
            config.Bind(() => literal.StringDE);

            Assert.AreEqual("ąęśćżźó", literal.StringPL);
            Assert.AreEqual("äöüß", literal.StringDE);

            literal.StringDE += "---";
            literal.StringPL += "---";

            config.Update(() => literal.StringPL);
            config.Update(() => literal.StringDE);

            config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });
            literal = new Literal();

            config.Bind(() => literal.StringPL);
            config.Bind(() => literal.StringDE);

            Assert.AreEqual("ąęśćżźó---", literal.StringPL);
            Assert.AreEqual("äöüß---", literal.StringDE);
        }

        [TestMethod]
        public void IO_Other()
        {
            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });

            var other = new Other();

            config.Bind(() => other.Boolean);
            config.Bind(() => other.Enum);
            config.Bind(() => other.DateTime);
            
            Assert.AreEqual(true, other.Boolean);
            Assert.AreEqual(TestEnum.TestValue2, other.Enum);
            Assert.AreEqual(new DateTime(2016, 7, 30), other.DateTime);

            other.Boolean = !other.Boolean;
            other.Enum = TestEnum.TestValue3;
            other.DateTime = other.DateTime.AddDays(-1);

            config.Update(() => other.Boolean);
            config.Update(() => other.Enum);
            config.Update(() => other.DateTime);

            config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });

            other = new Other();

            config.Bind(() => other.Boolean);
            config.Bind(() => other.Enum);
            config.Bind(() => other.DateTime);

            Assert.AreEqual(false, other.Boolean);
            Assert.AreEqual(TestEnum.TestValue3, other.Enum);
            Assert.AreEqual(new DateTime(2016, 7, 30).AddDays(-1), other.DateTime);
        }

        [TestMethod]
        public void IO_Painting()
        {
            //Assert.Inconclusive("Requires custom json-converters.");

            var config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });

            var paint = new Painting();

            config.Bind(() => paint.ColorName);
            config.Bind(() => paint.ColorDec);
            config.Bind(() => paint.ColorHex);
            config.Bind(() => paint.Window);

            Assert.AreEqual(Color.DarkRed.ToArgb(), paint.ColorName.ToArgb());
            Assert.AreEqual(Color.Plum.ToArgb(), paint.ColorDec.ToArgb());
            Assert.AreEqual(Color.Beige.ToArgb(), paint.ColorHex.ToArgb());
            Assert.AreEqual(Color.DarkCyan.ToArgb(), paint.Window.WindowColor.ToArgb());

            // modify

            paint.ColorHex = Color.Azure;
            paint.Window.WindowColor = Color.Moccasin;

            config.Update(() => paint.ColorHex);
            config.Update(() => paint.Window);

            // re-check

            config = new Configuration(options =>
            {
                options
                    .UseJsonConverter()
                    .UseMultiple(Datastores);
            });
            paint = new Painting();

            config.Bind(() => paint.ColorHex);
            config.Bind(() => paint.Window);

            Assert.AreEqual(Color.Azure.ToArgb(), paint.ColorHex.ToArgb());
            Assert.AreEqual(Color.Moccasin.ToArgb(), paint.Window.WindowColor.ToArgb());
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

    public static class TestConfigurationOptionsExtensions
    {
        public static IConfigurationProperties UseMultiple(this IConfigurationProperties properties, IEnumerable<ISettingDataStore> datastores)
        {
            properties.Datastores.AddRange(datastores);
            return properties;
        }
    }
}