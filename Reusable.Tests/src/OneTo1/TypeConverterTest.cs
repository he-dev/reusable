using System.Collections.Generic;
using Reusable.OneTo1.Converters;
using Reusable.OneTo1.Decorators;
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
            using var decorator =
                DecoratorScope
                    .For<ITypeConverter>()
                    .Add<SkipConverted>()
                    .Add<FriendlyException>();

            // Collection initializer uses DecoratorScope
            return new TypeConverterStack
            {
                new BooleanToString()
            };
        });

        [Fact]
        public void Skips_convert_when_type_already_converted()
        {
            using var decorator =
                DecoratorScope
                    .For<ITypeConverter>()
                    .Add<FriendlyException>()
                    .Add<SkipConverted>();

            // Push uses DecoratorScope
            var converter = TypeConverterStack.Empty.Push<BooleanToString>();
            Assert.Equal("True", converter.ConvertOrDefault<string>("True"));
        }
    }
}