using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Commander.Annotations;
using SoftKeySet = Reusable.Collections.ImmutableKeySet<Reusable.SoftString>;

namespace Reusable.Commander
{
    public interface ICommandRegistration : IEquatable<ICommandRegistration>, IEnumerable<CommandParameter>
    {
        [NotNull]
        [AutoEqualityProperty]
        SoftKeySet CommandName { get; }

        [NotNull]
        Type CommandType { get; }        
    }
    
    public class CommandRegistration : ICommandRegistration
    {
        private readonly IReadOnlyList<CommandParameter> _parameters;        

        private CommandRegistration(Type commandType)
        {
            CommandType = commandType;
            CommandName = NameFactory.CreateCommandName(commandType);
            _parameters = GetParameters(commandType).ToList();
            
            CommandParameterValidator.ValidateParameterNamesUniqueness(this);
        }

        public static ICommandRegistration Create<T>()
        {
            return new CommandRegistration(typeof(T));
        }

        #region ICommandRegistration

        public Type CommandType { get; }

        public SoftKeySet CommandName { get; }

        #endregion

        #region IEquatable

        public bool Equals(ICommandRegistration other) => AutoEquality<ICommandRegistration>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as ICommandRegistration);

        public override int GetHashCode() => AutoEquality<ICommandRegistration>.Comparer.GetHashCode(this);

        #endregion

        #region IEnumerable

        public IEnumerator<CommandParameter> GetEnumerator() => _parameters.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        [NotNull]
        private static IEnumerable<CommandParameter> GetParameters([NotNull] Type commandType)
        {
            return
                commandType
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .Where(property => property.IsDefined(typeof(ParameterAttribute)))
                    .Select(CommandParameter.Create);
        }
    }
}