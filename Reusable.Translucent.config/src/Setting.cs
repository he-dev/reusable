using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reusable.OneTo1;
using Reusable.Quickey;

namespace Reusable.Translucent
{
    [UseType, UseMember]
    [PlainSelectorFormatter]
    public abstract class Setting
    {
        private static readonly From<Setting>? This;

        public static Selector<ITypeConverter> Converter { get; } = This.Select(() => Converter);

        public static Selector<IEnumerable<ValidationAttribute>> Validations { get; } = This.Select(() => Validations);
    }
}