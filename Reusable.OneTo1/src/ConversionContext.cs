using Reusable.OneTo1.Converters.Specialized;

namespace Reusable.OneTo1
{
    public class ConversionContext
    {
        public ITypeConverter Converter { get; set; } = new Never();
    }
}