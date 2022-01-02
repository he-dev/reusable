using System.IO;
using System.Text;

namespace Reusable.Synergy.Requests;

public interface IReadFile : IRequest
{
    public string Name { get; set; }

    public FileShare Share { get; }
}

public abstract class ReadFile<T> : Request<T>, IReadFile
{
    protected ReadFile(string name) => Name = name;

    public string Name { get; set; }

    public FileShare Share { get; set; } = FileShare.Read;
}

public abstract class ReadFile
{
    public class Text : ReadFile<string>
    {
        public Text(string name) : base(name) { }

        public Encoding Encoding { get; set; } = Encoding.UTF8;
    }

    public class Stream : ReadFile<FileStream>
    {
        public Stream(string name) : base(name) { }
    }
}