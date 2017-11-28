using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog;

namespace Reusable.Commander
{
    public interface ICommandFactory
    {
        [CanBeNull]
        IConsoleCommand CreateCommand(SoftKeySet commandName, ICommandLine commandLine);
    }

    [UsedImplicitly]
    public class CommandFactory : ICommandFactory
    {
        private readonly ILogger _logger;
        private readonly IIndex<SoftKeySet, IConsoleCommand> _commands;
        private readonly ICommandParameterMapper _mapper;

        public CommandFactory(
            [NotNull] ILoggerFactory loggerFactory, 
            [NotNull] IIndex<SoftKeySet, IConsoleCommand> commands, 
            [NotNull] ICommandParameterMapper mapper)
        {
            _logger = loggerFactory.CreateLogger(nameof(CommandFactory));
            _commands = commands;
            _mapper = mapper;
        }

        public IConsoleCommand CreateCommand(SoftKeySet name, ICommandLine commandLine)
        {
            if (_commands.TryGetValue(name, out var command))
            {
                _logger.Debug(() => $"Created command {name.FirstLongest().ToString().QuoteWith("'")}.");
                return _mapper.Map(command, commandLine);
            }
            else
            {
                return
                    _logger
                        .Error(() => $"Could not find command {name.First().ToString().QuoteWith("'")}.")
                        .Return(default(IConsoleCommand));
            }
        }
    }

    public static class SoftKeySetExtensions
    {
        [NotNull]
        public static SoftString FirstLongest([NotNull] this SoftKeySet keys)
        {
            if (keys == null) throw new ArgumentNullException(nameof(keys));
            
            return keys.OrderByDescending(key => key.Length).First();
        }
    }
}