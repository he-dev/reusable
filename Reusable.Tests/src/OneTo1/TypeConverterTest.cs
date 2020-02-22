using System;
using System.Collections.Generic;
using System.Globalization;
using Reusable.Exceptionize;
using Reusable.OneTo1.Converters;
using Reusable.OneTo1.Converters.Specialized;
using Reusable.OneTo1.Decorators;
using Xunit;

namespace Reusable.OneTo1
{
    public partial class TypeConverterTest
    {
        private static readonly ITypeConverter ConverterForValueTypes = Factory.Create(() =>
        {
            using var _ = DecoratorScope.For<ITypeConverter>().Add<SkipConverted>().Add<FriendlyException>();

            return new TypeConverterStack
            {
                new StringToInt32(),
                new StringToBoolean(),
                new StringToDateTime(),
                new StringToTimeSpan(),
                //new Lambda((value, toType, context) => throw DynamicException.Create("Test", "This went wrong."))
                new Int32ToString(),
                new BooleanToString(),
                new DateTimeToString(),
                new TimeSpanToString(),
            };
        });

        [Theory]
        [MemberData(nameof(GetValueData))]
        public void Can_convert_value_types(object from, object to)
        {
            Assert.Equal(to, ConverterForValueTypes.ConvertOrThrow(from, to.GetType()));
        }

        public static IEnumerable<object[]> GetValueData()
        {
            yield return new object[] { "1", 1 };
            yield return new object[] { "True", true };
            yield return new object[] { "02/22/2020 10:20:30", new DateTime(2020, 2, 22, 10, 20, 30) };
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