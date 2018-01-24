using System;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Reusable.SmartConfig.SettingConverters.Json.Tests
{
    [TestClass]
    public class JsonSettingConverterTest
    {
        private static readonly ISettingConverter Converter = JsonSettingConverter.Default;

        private static readonly (object, string)[] TestValues = 
        {
            (SByte.MaxValue, "127"),
            (Byte.MaxValue, "255"),
            //("Numeric.Char": "￿", Char.MaxValue - 1),
            (Int16.MaxValue, "32767"),
            (Int32.MaxValue, "2147483647"),
            (Int64.MaxValue, "9223372036854775807"),
            (UInt16.MaxValue, "65535"),
            (UInt32.MaxValue, "4294967295"),
            (UInt64.MaxValue, "18446744073709551615"),
            (Single.MaxValue, "3.40282347E+38"),
            (Double.MaxValue, "1.7976931348623157E+308"),
            (Decimal.MaxValue, "79228162514264337593543950335.0"),
                
            //("Literal.StringDE": "äöüß",
            //("Literal.StringPL": "ąęśćżźó",

            ("foo", "foo"),

            (true, "true"),
            (TestEnum.TestValue2, "TestValue2"),
            (new DateTime(2016, 7, 30), "2016-07-30T00:00:00"),
                
            //(Color.DarkRed, "DarkRed"),
            //("Painting.ColorDec": "221,160,221", // Plum
            //(Color.Beige, "#F5F5DC"), // Beige
            //("Painting.Window": "{\"WindowColor\": \"DarkCyan\" }",
                
            //(new []{5, 8, 13}, "[5, 8, 13]"),
            //("Collection.ArrayInt32[0]": "5",
            //("Collection.ArrayInt32[1]": "8",
                
            //("Collection.DictionaryStringInt32[foo]": "21",
            //("Collection.DictionaryStringInt32[bar]": "34"
        };

        [TestMethod]
        public void Serialize_BasicTypes_Strings()
        {
            foreach (var (value, expected) in TestValues)
            {
                Assert.AreEqual(expected, Converter.Serialize(value));
            }
        }

        [TestMethod]
        public void Deserialize_BasicTypes_Values()
        {
            foreach (var (expected, value) in TestValues)
            {
                Assert.AreEqual(expected, Converter.Deserialize(value, expected.GetType()));
            }
        }

        public enum TestEnum
        {
            TestValue1,
            TestValue2,
            TestValue3
        }
    }
}
