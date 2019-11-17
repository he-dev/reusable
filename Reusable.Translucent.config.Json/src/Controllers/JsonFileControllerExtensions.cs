using Reusable.Translucent.Controllers;

// ReSharper disable once CheckNamespace
namespace Reusable.Translucent
{
    public static class JsonFileControllerExtensions
    {
        public static IResourceCollection AddJsonFile(this IResourceCollection controllers, string basePath, string fileName)
        {
            return controllers.Add(new JsonFileController(basePath, fileName));
        }
    }
}