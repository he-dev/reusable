using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.Commander.Annotations;

namespace Reusable.Commander
{
    public interface ICommandInfo : IEquatable<ICommandInfo>, IEnumerable<CommandParameter>
    {
        [NotNull]
        [AutoEqualityProperty]
        SoftKeySet CommandName { get; }

        [NotNull]
        Type CommandType { get; }
    }

    public class CommandInfo : ICommandInfo
    {
        private CommandInfo(Type commandType)
        {
            CommandType = commandType;
            CommandName = NameFactory.CreateCommandName(commandType);
            Parameters = GetParameters(commandType).ToImmutableList();

            CommandParameterValidator.ValidateParameterNamesUniqueness(this);
        }

        private IImmutableList<CommandParameter> Parameters { get; }

        #region ICommandRegistration

        public Type CommandType { get; }

        public SoftKeySet CommandName { get; }

        #endregion

        public static ICommandInfo Create<T>() => new CommandInfo(typeof(T));

        #region IEquatable

        public bool Equals(ICommandInfo other) => AutoEquality<ICommandInfo>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as ICommandInfo);

        public override int GetHashCode() => AutoEquality<ICommandInfo>.Comparer.GetHashCode(this);

        #endregion

        #region IEnumerable

        public IEnumerator<CommandParameter> GetEnumerator() => Parameters.GetEnumerator();

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