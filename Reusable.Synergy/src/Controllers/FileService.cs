using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;
using Reusable.Synergy.Requests;

namespace Reusable.Synergy.Controllers;

public abstract class FileService : Service
{
    protected FileService() => MustSucceed = true;

    public class Read : FileService
    {
        public override async Task<object> InvokeAsync(IRequest request)
        {
            if (request is not IReadFile file)
            {
                throw DynamicException.Create("UnknownRequest", $"{request.GetType().ToPrettyString()} is not supported by this {nameof(FileService)}.");
            }

            if (!File.Exists(file.Name))
            {
                return
                    MustSucceed
                        ? throw DynamicException.Create("FileNotFound", $"There is no such file as '{file.Name}'.")
                        : await InvokeNext(request);
            }

            if (request is ReadFile<string>)
            {
                //await using var reader = new StreamReader()
                await using var stream = new FileStream(file.Name, FileMode.Open, FileAccess.Read, file.Share);
                return await stream.ReadTextAsync(Encoding.UTF8);
            }


            return new FileStream(file.Name, FileMode.Open, FileAccess.Read, file.Share).ToTask();
        }
    }

    public class Write : FileService
    {
        public override async Task<object> InvokeAsync(IRequest request)
        {
            if (request is not IWriteFile file)
            {
                throw DynamicException.Create("UnknownRequest", $"{request.GetType().ToPrettyString()} is not supported by this {nameof(FileService)}.");
            }

            if (!File.Exists(file.Name))
            {
                return
                    MustSucceed
                        ? throw DynamicException.Create("FileNotFound", $"There is no such file as '{file.Name}'.")
                        : await InvokeNext(request);
            }

            if (file.Value is string text)
            {
                await using var writer = new StreamWriter(file.Name, false, Encoding.UTF8);
                await writer.WriteAsync(text);
            }

            if (file.Value is Stream source)
            {
                await using var target = new FileStream(file.Name, FileMode.Create, FileAccess.Write, FileShare.Write);
                await source.Rewind().CopyToAsync(target);
                await target.FlushAsync();
            }

            throw DynamicException.Create("UnknownRequest", $"{request.GetType().ToPrettyString()} is not supported by this {nameof(FileService)}.");
        }
    }
}