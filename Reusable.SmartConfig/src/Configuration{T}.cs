using JetBrains.Annotations;
using Reusable.IOnymous;

namespace Reusable.SmartConfig
{
    public interface IConfiguration<T> : IConfiguration { }

    public class Configuration<T> : Configuration, IConfiguration<T>
    {
        public Configuration([NotNull] IResourceProvider settingProvider) : base(settingProvider) { }
    }
}