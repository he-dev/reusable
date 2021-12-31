using System;
using System.IO;
using Reusable.Essentials;
using Reusable.Octopus.Data;
using Reusable.Octopus.Extensions;

namespace Reusable.Octopus;

public static class RequestConfiguration
{
    public static Action<T> Append<T>(this Action<T> configure) where T : FileRequest
    {
        return configure.Then(request => request.SetItem(FileMode.Append));
    }
}