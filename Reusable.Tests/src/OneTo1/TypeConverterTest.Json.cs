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
        private static readonly ITypeConverter ConverterForJson = Factory.Create(() =>
        {
            using var _ = DecoratorScope.For<ITypeConverter>().Add<SkipConverted>().Add<FriendlyException>();

            return new TypeConverterStack
            {
                new ObjectToJson(),
                new JsonToObject()
            };
        });

        [Theory]
        [MemberData(nameof(GetJsonData))]
        public void Can_convert_json_types(object from, object to)
        {
            Assert.Equal(to, ConverterForJson.ConvertOrThrow(from, to.GetType()));
        }

        public static IEnumerable<object[]> GetJsonData()
        {
            yield return new object[] { "1", 1 };
            yield return new object[] { "True", true };
            yield return new object[] { "02/22/2020 10:20:30", new DateTime(2020, 2, 22, 10, 20, 30) };
        }
    }
}