using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface ICommandCollection : IEnumerable<ICommandInfo>
    {
        ICommandCollection Add(ICommandInfo info);
    }

    public class CommandCollection : ICommandCollection
    {
        private readonly ISet<ICommandInfo> _commands;

        private CommandCollection()
        {
            _commands = new HashSet<ICommandInfo>();
        }

        public static ICommandCollection Empty => new CommandCollection();

        public ICommandCollection Add(ICommandInfo info)
        {
            if (!_commands.Add(info))
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"DuplicateCommand{nameof(Exception)}", 
                    $"There is already another command with the name {info.CommandName.FirstLongest().ToString()}.", 
                    null
                );
            }

            return this;
        }

        public IEnumerator<ICommandInfo> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    public static class CommandCollectionExtensions
    {
        public static ICommandCollection Add<T>(this ICommandCollection commands)
        {
            return commands.Add(CommandInfo.Create<T>());
        }
    }
}