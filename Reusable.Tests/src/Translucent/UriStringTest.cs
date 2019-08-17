using Xunit;

namespace Reusable.Translucent
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
        public void Can_automatically_encode_percent_character()
        {
            var uri = new UriString("scheme:%temp%");

            Assert.Equal("%25temp%25", uri.Path.Original);
        }

        [Fact]
        public void Can_add_absolute_and_relative_paths()
        {
            var absolute = new UriString("blub:///c:/temp");
            var relative = new UriString("more-temp/file.txt");
            var newAbsolute = absolute + relative.Path;

            Assert.Equal("blub:///c:/temp/more-temp/file.txt", (string)newAbsolute);
        }

        [Fact]
        public void Can_parse_authority_host_and_port()
        {
            var uri = new UriString("http://localhost:1234/resource");

            Assert.Equal("http", uri.Scheme);
            Assert.Equal("localhost:1234", uri.Authority);
            Assert.Equal("resource", uri.Path.Decoded.ToString());
        }

        [Fact]
        public void Considers_query_with_same_keys_and_values_equal_regardless_of_their_order()
        {
            var uri1 = new UriString("scheme://autho.rity/p/a/t/h?f=b&q=a#fragment");
            var uri2 = new UriString("scheme://autho.rity/p/a/t/h?q=a&f=b#fragment");

            Assert.True(uri1 == uri2);
        }

        [Fact]
        public void Considers_query_with_different_keys_and_values_not_equal_regardless_of_their_order()
        {
            var uri1 = new UriString("scheme://autho.rity/p/a/t/h?f=b&q=b#fragment");
            var uri2 = new UriString("scheme://autho.rity/p/a/t/h?q=a&f=b#fragment");

            Assert.True(uri1 == uri2);
        }
    }
}