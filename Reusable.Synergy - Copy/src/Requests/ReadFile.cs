using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Synergy.Requests;


public abstract class ReadFile<T> : Request<T>, IIdentifiable
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