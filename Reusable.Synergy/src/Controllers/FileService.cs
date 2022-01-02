using System.IO;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Synergy.Requests;

namespace Reusable.Synergy.Controllers;

public abstract class FileService : Service
{
    protected FileService()
    {
        MustSucceed = true;
    }

    public class Read : FileService
    {
        public override async Task<object> InvokeAsync(IRequest request)
        {
            if (request is IReadFile file)
            {
                if (File.Exists(file.Name))
                {
                    if (request is ReadFile.Text t)
                    {
                        await using var stream = new FileStream(file.Name, FileMode.Open, FileAccess.Read, file.Share);
                        return await stream.ReadTextAsync(t.Encoding);
                    }

                    if (request is ReadFile.Stream s)
                    {
                        return new FileStream(file.Name, FileMode.Open, FileAccess.Read, file.Share).ToTask();
                    }
                }
                else
                {
                    return
                        MustSucceed
                            ? throw DynamicException.Create("FileNotFound", $"There is no such file as '{file.Name}'.")
                            : await InvokeNext(request);
                }
            }

            throw DynamicException.Create("UnknownRequest", $"{request.GetType().ToPrettyString()} is not supported by this {nameof(FileService)}.");
        }
    }
}