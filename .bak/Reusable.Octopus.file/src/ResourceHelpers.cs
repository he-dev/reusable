using System;
using System.IO;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Data;


namespace Reusable.Octopus;

using static RequestMethod;

public static class ResourceHelpers
{
    public static IReadBuilder<FileRequest> File(this RequestMethodBuilder.Read builder, string name)
    {
        return new Request.Builder<FileRequest>(builder.Resource, new FileRequest
        {
            Schema = nameof(File),
            ResourceName = { name },
            Method = Read
        });
    }
    
    public static ICreateBuilder<FileRequest> File(this RequestMethodBuilder.Create builder, string name)
    {
        return new Request.Builder<FileRequest>(builder.Resource, new FileRequest
        {
            Schema = nameof(File),
            ResourceName = { name },
            Method = Read
        });
    }
    
    public static ICreateBuilder<T> Data<T>(this ICreateBuilder<T> builder, object data) where T : FileRequest
    {
        return builder.Also(b => b.Request.Data.Push(data));
    }
}