using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.IOnymous;

namespace Reusable.Commander
{
    internal class CommandArgumentProvider : ResourceProvider
    {
        public new const string DefaultScheme = "command-line";

        private readonly ICommandLine _commandLine;

        public CommandArgumentProvider([NotNull] ICommandLine commandLine) : base(new SoftString[] { DefaultScheme }, ImmutableSession.Empty)
        {
            _commandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            if (uri.TryGetDataString("name", out var name))
            {
                var id = Identifier.FromName(name);
                var parameter = _commandLine[id];
                return Task.FromResult<IResourceInfo>
                (
                    parameter is null
                        ? new CommandArgumentInfo(uri, false, new List<string>())
                        : new CommandArgumentInfo(uri, true, parameter.ToList())
                );
            }

            throw new ArgumentException($"{nameof(uri)}'s query-string must contain the 'name' parameter.");
        }
    }
}