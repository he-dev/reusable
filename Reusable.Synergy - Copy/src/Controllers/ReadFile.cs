using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Synergy.Controllersx;

public abstract class ReadFile<T> : Service<T>
{
    protected ReadFile(string name) => Name = name;

    public string Name { get; set; }

    public FileShare Share { get; set; } = FileShare.Read;
}

public static class ReadFile
{
    public class Text : ReadFile<string>
    {
        public Text(string name) : base(name) { }

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public override async Task<string> InvokeAsync()
        {
            if (File.Exists(Name))
            {
                await using var fileStream = new FileStream(Name, FileMode.Open, FileAccess.Read, Share);
                return await fileStream.ReadTextAsync(Encoding);
            }
            else
            {
                throw DynamicException.Create("FileNotFound", $"There is no such file as '{Name}'.");
            }
        }
    }
    
    public class Stream : ReadFile<FileStream>
    {
        public Stream(string name) : base(name) { }

        public override Task<FileStream> InvokeAsync()
        {
            if (File.Exists(Name))
            {
                return new FileStream(Name, FileMode.Open, FileAccess.Read, Share).ToTask();
            }
            else
            {
                throw DynamicException.Create("FileNotFound", $"There is no such file as '{Name}'.");
            }
        }
    }
}