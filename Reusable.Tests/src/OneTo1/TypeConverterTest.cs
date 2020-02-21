using System.Collections.Generic;
using Reusable.OneTo1.Converters;
using Xunit;

namespace Reusable.OneTo1
{
    public class TypeConverterTest
    {
        private static readonly ITypeConverter Converter = new CompositeConverter
        {
            new StringToInt32()
        };
        
        [Theory]
        [MemberData(nameof(GetData))]
        public void Can_convert(object from, object to)
        {
            Assert.Equal(to, Converter.Convert(from, to.GetType()));
        }
        
        public static IEnumerable<object[]> GetData()
        {
            yield return new object[] { "1", 1 };
        }
    }
}