using Reusable.Snowball.Converters.Specialized;

namespace Reusable.Snowball;

public class ConversionContext
{
    public ITypeConverter Converter { get; set; } = new Never();
}