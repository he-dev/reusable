using System.ComponentModel.DataAnnotations.Schema;

namespace Reusable.Synergy;

public interface ISetting
{
    string Name { get; set; }

    string Value { get; set; }
}

[Table("Setting", Schema = "dbo")]
public class Setting : ISetting
{
    public string Name { get; set; } = default!;

    public string Value { get; set; } = default!;
}