using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Converters;
using Reusable.Tests.Helpers;

namespace Reusable.Tests.Converters
{
    [TestClass]
    public class JsonTest
    {
        [TestMethod]
        public void Convert_JsonTypeName_Interface()
        {
            var json = $@"{{ ""$type"": ""{typeof(Foo2).FullName}, {AssemblyInfo.Namespace}"" }}";

            var converter = TypeConverter.Empty
                .Add<JsonToObjectConverter<Foo>>();

            var foo = converter.Convert(json, typeof(Foo));

            Assert.IsInstanceOfType(foo, typeof(Foo2));
        }

        [TestMethod]
        public void Convert_JsonTypeName_AbstractClass()
        {
            //var json = $@"{{ ""$type"": ""{typeof(Bar1).FullName}, {typeof(Bar1).Namespace}"" }}";
            var json = $@"{{ ""$type"": ""{typeof(Bar1).FullName}, {AssemblyInfo.Namespace}"" }}";

            var converter = TypeConverter.Empty
                .Add<JsonToObjectConverter<Bar>>();

            var bar = converter.Convert(json, typeof(Bar));

            Assert.IsInstanceOfType(bar, typeof(Bar1));
        }

        [TestMethod]
        public void Convert_JsonArray_ArrayInt32()
        {
            var json = @"[1, 2, 3]";

            var converter = TypeConverter.Empty
                .Add<JsonToObjectConverter<int[]>>()
                .Add<StringToInt32Converter>();

            var result = converter.Convert(json, typeof(int[])) as int[];

            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, result);
        }
    }

    internal interface Foo { }

    internal class Foo1 : Foo { }

    internal class Foo2 : Foo { }

    internal abstract class Bar { }

    internal class Bar1 : Bar { }

    internal class Bar2 : Bar { }
}
