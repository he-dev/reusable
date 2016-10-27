using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Testing;
using Reusable.Validations;

namespace Reusable.TypeConversion.Tests
{
    [TestClass]
    public class JsonConvertersTests
    {
        [TestMethod]
        public void Convert_ToInterface()
        {
            var json = @"{ ""$type"": ""SmartUtilities.Tests.Frameworks.InfiniteConversion.Converters.Foo2, SmartUtilities.Tests"" }";

            var converter = TypeConverter.Empty.Add<JsonToObjectConverter<Foo>>();

            var foo = converter.Convert(json, typeof(Foo));
            foo.Verify().IsInstanceOfType(typeof(Foo2));
        }

        [TestMethod]
        public void Convert_ToAbstractClass()
        {
            var json = @"{ ""$type"": ""SmartUtilities.Tests.Frameworks.InfiniteConversion.Converters.Bar1, SmartUtilities.Tests"" }";

            var converter = TypeConverter.Empty.Add<JsonToObjectConverter<Bar>>();

            var bar = converter.Convert(json, typeof(Bar));
            bar.Verify().IsInstanceOfType(typeof(Bar1));
        }

        [TestMethod]
        public void Convert_Array()
        {
            var json = @"[1, 2, 3]";

            var converter = TypeConverter.Empty.Add<JsonToObjectConverter<int[]>>().Add<StringToInt32Converter>();

            var result = converter.Convert(json, typeof(int[])) as int[];
            result.Verify().IsNotNull().SequenceEqual(new int[] { 1, 2, 3 });
        }
    }

    internal interface Foo { }

    internal class Foo1 : Foo { }

    internal class Foo2 : Foo { }

    internal abstract class Bar { }

    internal class Bar1 : Bar { }

    internal class Bar2 : Bar { }
}
