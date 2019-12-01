using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class AppSettingControllerExtensions
    {
        public static IResourceCollection AddAppConfig(this IResourceCollection controllers, ComplexName? name = default)
        {
            return controllers.Add(new AppSettingController(name ?? ComplexName.Empty));
        }
    }
}