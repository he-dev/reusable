using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingFinder
    {
        bool TryFindSetting
        (
            [NotNull] GetValueQuery getValueQuery,
            [NotNull] IEnumerable<ISettingProvider> providers,
            out (ISettingProvider SettingProvider, ISetting Setting) result
        );
    }
}