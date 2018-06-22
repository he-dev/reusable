using System.Collections.Generic;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Utilities
{
    public interface ISettingFile
    {
        IList<ISetting> Settings { get; }
    }

    public abstract class SettingFile : ISettingFile
    {
        public IList<ISetting> Settings { get; }
    }
}
