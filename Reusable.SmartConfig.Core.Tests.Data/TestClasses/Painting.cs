using System.Drawing;
using Reusable.Formatters;

namespace Reusable.SmartConfig.Tests.Common.TestClasses
{
    public class Painting
    {
        //[Format("#RRGGBB", typeof(HexadecimalColorFormatter))]
        public Color ColorName { get; set; }

        //[Format("#RRGGBB", typeof(HexadecimalColorFormatter))]
        public Color ColorDec { get; set; }

        //[Format("#RRGGBB", typeof(HexadecimalColorFormatter))]
        public Color ColorHex { get; set; }

        public WindowConfig Window { get; set; }
    }

    public class WindowConfig
    {
        public Color WindowColor { get; set; }
    }
}