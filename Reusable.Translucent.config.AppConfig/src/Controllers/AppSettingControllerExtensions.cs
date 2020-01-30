using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class AppSettingControllerExtensions
    {
        public static IResourceCollection AddAppConfig(this IResourceCollection controllers, ControllerName? name = default)
        {
            return controllers.Add(new AppSettingController(name ?? ControllerName.Empty));
        }
    }
}