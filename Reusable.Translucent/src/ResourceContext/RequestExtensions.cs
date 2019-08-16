using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class RequestExtensions
    {
        [ItemNotNull]
        public static Task<Stream> CreateBodyStreamAsync(this Request request)
        {
            switch (request.Body)
            {
                case null: return Stream.Null.ToTask();
                case Stream stream: return stream.ToTask();
                case string text: return text.ToStream().ToTask();
                case CreateBodyStreamDelegate createBodyStream: return createBodyStream();
                default: throw new ArgumentOutOfRangeException($"Body of type '{request.Body.GetType().ToPrettyString()}' is not supported.");
            }
        }
    }
}