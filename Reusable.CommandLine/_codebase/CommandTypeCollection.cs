using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SmartCommandLine
{
    public class CommandTypeCollection : IEnumerable<Type>
    {
        private readonly List<Type> _commandTypes = new List<Type>();

        //public CommandTypeCollection(bool isImplicit)
        //{
        //    IsImplicit = isImplicit;
        //}

        public Type this[string commandName]
        {
            get
            {
                if (string.IsNullOrEmpty(commandName))
                {
                    return null;
                }
                return (
                    from commandType in _commandTypes
                    let commandNemaspace = string.Join(".", commandType.Namespaces().Concat(new[] { string.Empty }).ToList())
                    let commandNames = string.Join("|", commandType.Names().Select(Regex.Escape))
                    let commandMatcher = new Regex($"{commandNemaspace}({commandNames})", RegexOptions.IgnoreCase)
                    where commandMatcher.IsMatch(commandName)
                    select commandType
                ).FirstOrDefault();
            }
        }

        //public bool IsImplicit { get; }

        internal void Add<TCommand>() where TCommand : Command, new()
        {
            if (IsImplicit && _commandTypes.Any())
            {
                throw new InvalidOperationException("You can add only one command in the implicit mode.");
            }

            var commandFullNames = _commandTypes.SelectMany(t => t.FullNames()).ToList();
            var nameCollision = typeof(TCommand).FullNames().FirstOrDefault(n => commandFullNames.Contains(n, StringComparer.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(nameCollision))
            {
                throw new ArgumentException($"Command '{typeof(TCommand).FullName}' cannot be added because there is already another command with this name: '{nameCollision}'");
            }

            var duplicateParameterNames =
                typeof(TCommand).Parameters()
                    .SelectMany(x => x.Names)
                    .GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

            if (duplicateParameterNames.Any())
            {
                throw new ArgumentException($"Command '{typeof(TCommand).FullName}' has some duplicate paramter names: [{string.Join(", ", duplicateParameterNames)}].");
            }

            _commandTypes.Add(typeof(TCommand));
        }

        public IEnumerator<Type> GetEnumerator() => _commandTypes.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
