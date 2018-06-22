using System;
using System.Collections;
using System.Collections.Generic;
using Reusable.Reflection;

namespace Reusable.Commander
{
    public interface ICommandRegistrationContainer : IEnumerable<ICommandRegistration>
    {
        ICommandRegistrationContainer Register(ICommandRegistration registration);
    }
    
    public class CommandRegistrationContainer : ICommandRegistrationContainer
    {
        private readonly ISet<ICommandRegistration> _commands;

        private CommandRegistrationContainer()
        {
            _commands = new HashSet<ICommandRegistration>();
        }

        public static ICommandRegistrationContainer Empty => new CommandRegistrationContainer();

        public ICommandRegistrationContainer Register(ICommandRegistration registration)
        {
            if (!_commands.Add(registration))
            {
                throw DynamicException.Factory.CreateDynamicException($"DuplicateCommand{nameof(Exception)}", $"There is already another command with the name {registration.CommandName.ToString()}.", null);
            }

            return this;
        }

        public IEnumerator<ICommandRegistration> GetEnumerator() => _commands.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
    
    public static class CommandCollectionExtensions
    {
        public static ICommandRegistrationContainer Register<T>(this ICommandRegistrationContainer commands)
        {
            return commands.Register(CommandRegistration.Create<T>());
        }
    }
}