using Reusable.Data;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class SmtpControllerExtensions
    {
        public static IResourceControllerBuilder AddSmtp(this IResourceControllerBuilder builder, IImmutableContainer properties = default)
        {
            return builder.AddController(new SmtpController(properties));
        }
    }
}