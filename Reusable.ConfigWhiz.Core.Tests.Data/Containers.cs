using System;
using System.Collections.Generic;
using System.Drawing;
using Reusable.ConfigWhiz.Data.Annotations;
using Reusable.Data.Annotations;
using Reusable.StringFormatting.Formatters;
using Reusable.TypeConversion;

namespace Reusable.ConfigWhiz.Core.Tests.Data
{
    public class Bar
    {
        public string Baz { get; set; }
    }

    public class Numeric
    {
        public SByte SByte { get; set; }
        public Byte Byte { get; set; }
        public Char Char { get; set; }
        public Int16 Int16 { get; set; }
        public Int32 Int32 { get; set; }
        public Int64 Int64 { get; set; }
        public UInt16 UInt16 { get; set; }
        public UInt32 UInt32 { get; set; }
        public UInt64 UInt64 { get; set; }
        [Format("R")]
        public Single Single { get; set; }
        [Format("R")]
        public Double Double { get; set; }
        public Decimal Decimal { get; set; }
    }

    public class Literal
    {
        public String StringDE { get; set; }
        public String StringPL { get; set; }
    }

    public class Other
    {
        public Boolean Boolean { get; set; }
        public TestEnum Enum { get; set; }
        public DateTime DateTime { get; set; }

        [Ignore]
        public string IgnoreString { get; set; } = "Ignore value";
    }

    public class Paint
    {
        [Format("#RRGGBB", typeof(HexadecimalColorFormatter))]
        public Color ColorName { get; set; }

        [Format("#RRGGBB", typeof(HexadecimalColorFormatter))]
        public Color ColorDec { get; set; }

        [Format("#RRGGBB", typeof(HexadecimalColorFormatter))]
        public Color ColorHex { get; set; }        
    }

    [TypeConverter(typeof(JsonToObjectConverter<List<Int32>>))]
    [TypeConverter(typeof(ObjectToJsonConverter<List<Int32>>))]
    public class Collection
    {        
        public List<int> JsonArray { get; set; }

        [Itemized]
        public int[] ArrayInt32 { get; set; }

        [Itemized]
        public Dictionary<string, int> DictionaryStringInt32 { get; set; }
    }

    public enum TestEnum
    {
        TestValue1,
        TestValue2,
        TestValue3
    }
}
