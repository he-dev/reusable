using Xunit;

namespace Reusable.Translucent
{
    public class UriStringHelperTest
    {
        [Fact]
        public void Can_encode_and_decode_reserved_characters()
        {
            var encoded = UriStringHelper.Encode("%temp%");
            
            Assert.Equal("%25temp%25", encoded);
            
            var decoded = UriStringHelper.Decode(encoded);
            
            Assert.Equal("%temp%", decoded);                       
        }
    }
}