using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class AppSettingControllerExtensions
    {
        public static IResourceControllerBuilder UseAppConfig(this IResourceControllerBuilder builder)
        {
            return builder.AddController(new AppSettingController());
        }
    }
}