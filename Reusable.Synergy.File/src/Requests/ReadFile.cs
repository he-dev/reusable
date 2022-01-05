using System.IO;
using System.Text;

namespace Reusable.Synergy.Requests;

public interface IReadFile : IRequest
{
    public string Name { get; set; }
    
    public FileShare Share { get; set; }
}

public class ReadFile<T> : Request<T>, IReadFile
{
    public ReadFile(string name) => Name = name;

    public string Name { get; set; }

    public FileShare Share { get; set; } = FileShare.Read;
}