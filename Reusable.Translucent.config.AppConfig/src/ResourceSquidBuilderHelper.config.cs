using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class ResourceSquidBuilderExtensions
    {
        public static ResourceSquidBuilder UseAppConfig(this ResourceSquidBuilder builder)
        {
            return builder.UseController(new AppSettingController());
        }
    }
}