using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.Quickey;
using Reusable.Reflection;

namespace Reusable.Commander.Services
{
    internal class CommandParameterProvider : ResourceProvider
    {
        public new const string DefaultScheme = "command-line";

        private readonly ICommandLine _commandLine;

        public CommandParameterProvider([NotNull] ICommandLine commandLine) : base(new SoftString[] { DefaultScheme }, ImmutableSession.Empty)
        {
            _commandLine = commandLine ?? throw new ArgumentNullException(nameof(commandLine));
        }

        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            var exists = TryGetValues(uri, out var values);
            var parameterType = metadata.GetItemOrDefault(From<ICommandParameterMeta>.Select(x => x.ParameterType));
            return Task.FromResult<IResourceInfo>(new CommandParameterInfo(uri, exists, parameterType.IsList() ? values : values.Take(1).ToList()));
        }

        private (bool Exists, List<string> Values) GetValues(UriString uri)
        {
            if (uri.Query.TryGetValue(CommandArgumentQueryStringKeys.Position, out var p))
            {
                var position = int.Parse((string)p);
                var elementAtOrDefault = _commandLine.AnonymousParameter().ElementAtOrDefault(position);
                var exists = !(elementAtOrDefault is null);
                return
                (
                    exists,
                    exists
                        ? new List<string> { elementAtOrDefault }
                        : new List<string>()
                );
            }
            else
            {
                var names = uri.Path.Decoded.ToString().Split('/');
                var id = new Identifier(names.Select(n => new Name(n)));
                return
                    _commandLine.Contains(id)
                        ? (true, _commandLine[id].ToList())
                        : (false, new List<string>());
            }
        }

        private bool TryGetValues(UriString uri, out List<string> values)
        {
            values = new List<string>();

            if (uri.Query.TryGetValue(CommandArgumentQueryStringKeys.Position, out var p))
            {
                var position = int.Parse((string)p);
                var elementAtOrDefault = _commandLine.AnonymousParameter().ElementAtOrDefault(position);

                if (elementAtOrDefault is null)
                {
                    return false;
                }

                values = new List<string> { elementAtOrDefault };
                return true;
            }

            if (uri.TryGetDataString("name", out var name))
            {
                var id = new Identifier(new Name(name));

                if (_commandLine.Contains(id))
                {
                    values = _commandLine[id].ToList();
                    return true;
                }
            }

            return false;
        }
    }

    public static class CommandArgumentQueryStringKeys
    {
        public const string Position = nameof(Position);
        //public const string IsCollection = nameof(IsCollection);
    }
}