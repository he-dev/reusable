using System.IO;
using System.Text;
using System.Threading.Tasks;
using Autofac.Core;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;

namespace Reusable.Synergy.Controllers;

using Reusable.Synergy.Requests;

public abstract class FileNode : Node
{
    public class Read : FileNode
    {
        public override async Task<object> InvokeAsync(IRequest request)
        {
            if (request is IIdentifiable identifiable && !File.Exists(identifiable.Name))
            {
                return request.GetItem<OnError>() switch
                {
                    OnError.Halt => throw DynamicException.Create("FileNotFound", $"There is no such file as '{identifiable.Name}'."),
                    OnError.Next => await InvokeNext(request)
                };
            }

            if (request is ReadFile.Text t)
            {
                await using var fileStream = new FileStream(t.Name, FileMode.Open, FileAccess.Read, t.Share);
                return await fileStream.ReadTextAsync(t.Encoding);
            }

            if (request is ReadFile.Stream s)
            {
                return new FileStream(s.Name, FileMode.Open, FileAccess.Read, s.Share).ToTask();
            }


            throw DynamicException.Create("UnknownRequest", $"{request.GetType().ToPrettyString()} is not supported by this {nameof(FileNode)}.");
        }
    }
}