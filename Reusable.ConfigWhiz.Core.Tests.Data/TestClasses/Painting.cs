using System.Drawing;
using Reusable.Data.Annotations;
using Reusable.StringFormatting.Formatters;

namespace Reusable.SmartConfig.Tests.Common.TestClasses
{
    public class Painting
    {
        [Format("#RRGGBB", typeof(HexadecimalColorFormatter))]
        public Color ColorName { get; set; }

        [Format("#RRGGBB", typeof(HexadecimalColorFormatter))]
        public Color ColorDec { get; set; }

        [Format("#RRGGBB", typeof(HexadecimalColorFormatter))]
        public Color ColorHex { get; set; }        
    }
}