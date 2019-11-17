using Reusable.Data;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class SmtpControllerExtensions
    {
        public static IResourceCollection AddSmtp(this IResourceCollection controllers, IImmutableContainer properties = default)
        {
            return controllers.Add(new SmtpController(properties));
        }
    }
}