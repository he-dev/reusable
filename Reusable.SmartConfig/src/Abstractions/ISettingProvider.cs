using System;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    public interface ISettingProvider : IEquatable<ISettingProvider>
    {
        [NotNull]
        [AutoEqualityProperty]
        SoftString Name { get; }        

        [CanBeNull]
        ISetting Read([NotNull] SelectQuery query);

        void Write([NotNull] UpdateQuery query);
    }
}