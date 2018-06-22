using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Validation
{
    public static class DataFuseExtensions
    {
        [NotNull]
        public static DataFuseContext<T> ValidateWith<T>([NotNull] this T obj, [NotNull] IDataFuse<T> validator)
        {
            return new DataFuseContext<T>(obj, validator.Validate(obj).ToList());
        }
    }
}