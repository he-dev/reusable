using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class UriStringTest
    {
        [Fact]
        public void Can_be_created_from_string()
        {
            var uri = new UriString("scheme://autho.rity/p/a/t/h?query=string#fragment");
            
            Assert.Equal("scheme", uri.Scheme);
            Assert.Equal("autho.rity", uri.Authority);
            Assert.Equal("p/a/t/h", uri.Path.Original);
            Assert.Equal("string", uri.Query["query"]);
            Assert.Equal("fragment", uri.Fragment);
            
            Assert.Equal("scheme://autho.rity/p/a/t/h?query=string#fragment", uri);
        }
        
        [Fact]
        public void Is_comparable_with_equals_operator()
        {
            var uri1 = new UriString("scheme://autho.rity/p/a/t/h?query=string#fragment");
            var uri2 = new UriString("scheme://autho.rity/p/a/t/h?query=string#fragment");
            
            Assert.True(uri1 == uri2);
            
            var uri3 = new UriString("scheme://autho.rity/p/a/t/h/s?query=string#fragment");
            
            Assert.False(uri1 == uri3);
        }
        
        [Fact]
        public void Ignores_scheme_when_one_is_ionymous()
        {
            var uri1 = new UriString("scheme://autho.rity/p/a/t/h?query=string#fragment");
            var uri2 = new UriString("ionymous://autho.rity/p/a/t/h?query=string#fragment");
            
            Assert.True(uri1 == uri2);
            
            var uri3 = new UriString("scheme://autho.rity/p/a/t/h/s?query=string#fragment");
            
            Assert.False(uri1 == uri3);
        }

        [Fact]
        public void Can_automatically_encode_percent_character()
        {
            var uri = new UriString("scheme:%temp%");
            
            Assert.Equal("%25temp%25", uri.Path.Original);
        }
    }
}