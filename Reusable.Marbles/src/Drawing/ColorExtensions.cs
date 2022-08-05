using System;
using System.Collections.Generic;
using System.Drawing;

namespace Reusable.Marbles.Drawing;

public static class ColorExtensions
{
    private static readonly Dictionary<string, Func<Color, byte>> ColorComponents = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(Color.A)] = c => c.A,
        [nameof(Color.R)] = c => c.R,
        [nameof(Color.G)] = c => c.G,
        [nameof(Color.B)] = c => c.B,
    };

    public static byte Component(this Color color, string name) => ColorComponents[name](color);
}