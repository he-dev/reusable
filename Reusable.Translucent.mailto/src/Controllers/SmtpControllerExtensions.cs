using System;
using Reusable.Extensions;

namespace Reusable.Translucent.Controllers
{
    public static class SmtpControllerExtensions
    {
        public static IResourceCollection AddSmtp(this IResourceCollection controllers, string? id = default, Action<SmtpController>? configureController = default)
        {
            return controllers.Add(new SmtpController(id).Configure(configureController));
        }
    }
}