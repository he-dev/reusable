using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Extensions
{
    public static class RequestExtensions
    {
        [ItemNotNull]
        public static Task<Stream> CreateBodyStreamAsync(this Request request)
        {
            return request.Body switch
            {
                null => Stream.Null.ToTask(),
                Stream stream => stream.ToTask(),
                string text => text.ToStream().ToTask(),
                CreateBodyStreamDelegate createBodyStream => createBodyStream(),
                _ => throw new ArgumentOutOfRangeException($"Body of type '{request.Body.GetType().ToPrettyString()}' is not supported.")
            };
        }
    }
}