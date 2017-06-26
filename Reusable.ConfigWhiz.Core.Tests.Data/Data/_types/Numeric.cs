using System;
using Reusable.Data.Annotations;

namespace Reusable.ConfigWhiz.Tests.Common.Data
{
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
}