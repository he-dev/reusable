using System.Collections.Generic;
using Reusable.OneTo1.Converters;
using Xunit;

namespace Reusable.OneTo1
{
    public class TypeConverterTest
    {
        private static readonly ITypeConverter Converter = new TypeConverterStack
        {
            new StringToInt32()
        };

        [Theory]
        [MemberData(nameof(GetData))]
        public void Can_convert(object from, object to)
        {
            Assert.Equal(to, Converter.ConvertOrDefault(from, to.GetType()));
        }

        public static IEnumerable<object[]> GetData()
        {
            yield return new object[] { "1", 1 };
        }
    }

    public class DecoratorTest
    {
        private static readonly ITypeConverter Converter = Factory.Create(() =>
        {
            using var decorator = Decorator<ITypeConverter>.BeginScope
            (
                i => new SkipConvert(i),
                i => new TypeConvertException(i)
            );

            return
                TypeConverterStack
                    .Empty
                    .Push(decorator.Decorate<BooleanToString>());
        });

        [Fact]
        public void Skips_convert_when_type_already_converted()
        {
            using var decorator = Decorator<ITypeConverter>.BeginScope
            (
                i => new SkipConvert(i),
                i => new TypeConvertException(i)
            );

            var converter = decorator.Decorate<BooleanToString>();
            Assert.Equal("True", converter.ConvertOrDefault<string>("True"));
        }
    }
}