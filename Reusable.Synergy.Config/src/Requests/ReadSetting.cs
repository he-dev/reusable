using System.IO;
using System.Text;

namespace Reusable.Synergy.Requests;

public interface IReadSetting : IRequest
{
    public string Name { get; set; }
}

public class ReadSetting<T> : Request<T>, IReadSetting
{
    public ReadSetting(string name) => Name = name;

    public string Name { get; set; }
}