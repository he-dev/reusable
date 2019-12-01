using System;
using Reusable.Extensions;

namespace Reusable.Translucent.Controllers
{
    public static class SmtpControllerExtensions
    {
        public static IResourceCollection AddSmtp(this IResourceCollection controllers, ComplexName name = default, Action<SmtpToController>? configureController = default)
        {
            return controllers.Add(new SmtpToController(name ?? ComplexName.Empty).Pipe(configureController));
        }
    }
}