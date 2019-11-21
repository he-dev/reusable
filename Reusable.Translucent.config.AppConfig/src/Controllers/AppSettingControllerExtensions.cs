using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class AppSettingControllerExtensions
    {
        public static IResourceCollection AddAppConfig(this IResourceCollection controllers, string? id = default)
        {
            return controllers.Add(new AppSettingController(id));
        }
    }
}