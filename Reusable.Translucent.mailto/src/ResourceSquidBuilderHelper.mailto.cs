using Reusable.Data;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ResourceSquidBuilderExtensions
    {
        public static ResourceSquidBuilder UseSmtp(this ResourceSquidBuilder builder, IImmutableContainer properties = default)
        {
            return builder.UseController(new SmtpController(properties));
        }
    }
}