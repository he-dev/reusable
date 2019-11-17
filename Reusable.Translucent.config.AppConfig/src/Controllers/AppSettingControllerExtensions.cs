using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class AppSettingControllerExtensions
    {
        public static IResourceCollection AddAppConfig(this IResourceCollection controllers)
        {
            return controllers.Add(new AppSettingController());
        }
        
//        public static IResourceController ConfigureAppConfig(this IResourceRepositorySetup setup)
//        {
//            return new AppSettingController();
//        }
    }
}