using Reusable.Utilities.JsonNet.Annotations;
using Xunit;

namespace Reusable.Utilities.JsonNet
{
    public class TypeDictionaryTest
    {
        [Fact]
        public void Can_create_from_type_decorated_with_NamespaceAttribute()
        {
            var types = PrettyTypeDictionary.From(typeof(DecoratedType));
            Assert.True(types.ContainsKey($"Test.{nameof(DecoratedType)}"));
        }
        
        [Namespace("Test")]
        private class DecoratedBase {}
        
        [Namespace("Test")]
        private class DecoratedType : DecoratedBase
        {
            
        }
        
        [Fact]
        public void Can_create_from_type_decorated_with_attribute_derived_from_NamespaceAttribute()
        {
            var types = PrettyTypeDictionary.From(typeof(DecoratedType2));
            Assert.True(types.ContainsKey($"TestDerived.{nameof(DecoratedType2)}"));
        }
        
        public class TestDerivedAttribute : NamespaceAttribute
        {
            public TestDerivedAttribute() : base("TestDerived") { }
        }
        
        [TestDerived]
        private class DecoratedType2 : DecoratedBase
        {
            
        }
    }
}