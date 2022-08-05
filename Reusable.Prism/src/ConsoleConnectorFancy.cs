namespace Reusable.Prism;

public readonly record struct Quote(char Left, char Right)
{
    public static Quote Empty { get; } = new(default, default);

    public static Quote Single { get; } = new('\'', '\'');

    public static Quote Double { get; } = new('"', '"');

    public static Quote Square { get; } = new('[', ']');

    public static Quote Round { get; } = new('(', ')');

    public static Quote Curly { get; } = new('{', '}');

    public static Quote Angle { get; } = new('<', '>');
}