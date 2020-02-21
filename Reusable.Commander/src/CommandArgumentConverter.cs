using Reusable.OneTo1;
using Reusable.OneTo1.Converters;
using Reusable.OneTo1.Converters.Collections.Generic;

namespace Reusable.Commander
{
    public static class CommandArgumentConverter
    {
        public static readonly ITypeConverter Default = new CompositeConverter
        {
            new StringToSByte(),
            new StringToByte(),
            new StringToChar(),
            new StringToInt16(),
            new StringToInt32(),
            new StringToInt64(),
            new StringToUInt16(),
            new StringToUInt32(),
            new StringToUInt64(),
            new StringToSingle(),
            new StringToDouble(),
            new StringToDecimal(),
            new StringToColor(),
            new StringToBoolean(),
            new StringToDateTime(),
            new StringToTimeSpan(),
            new StringToEnum(),
            new EnumerableToList(),
        };
    }
}