using System;
using Reusable.Extensions;

namespace Reusable.Translucent.Controllers
{
    public static class SmtpControllerExtensions
    {
        public static IResourceCollection AddSmtp(this IResourceCollection controllers, ControllerName controllerName = default, Action<SmtpToController>? configureController = default)
        {
            return controllers.Add(new SmtpToController(controllerName ?? ControllerName.Empty).Pipe(configureController));
        }
    }
}