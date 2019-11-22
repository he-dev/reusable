using System;
using Reusable.Extensions;

namespace Reusable.Translucent.Controllers
{
    public static class SmtpControllerExtensions
    {
        public static IResourceCollection AddSmtp(this IResourceCollection controllers, string? id = default, Action<SmtpToController>? configureController = default)
        {
            return controllers.Add(new SmtpToController(id).Pipe(configureController));
        }
    }
}