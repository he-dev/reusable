using System;
using Reusable.Essentials.Collections;

namespace Reusable.DoubleDash;

public class CommandInfo : IEquatable<CommandInfo>
{
    public CommandInfo(Type commandType, NameCollection? name = default)
    {
        CommandType = commandType;
        NameCollection = name ?? commandType.GetArgumentName();
        ParameterType = commandType.GetCommandParameterType();

        //ValidateParameterPropertyNames(typeof(TParameter));
    }

    [AutoEqualityProperty]
    public NameCollection NameCollection { get; }

    public Type CommandType { get; }

    public Type ParameterType { get; }

    public string RegistrationKey => $"Commands.{NameCollection.Primary}";

    public override int GetHashCode() => AutoEquality<CommandInfo>.Comparer.GetHashCode(this);

    public override bool Equals(object obj) => Equals(obj as CommandInfo);

    public bool Equals(CommandInfo other) => AutoEquality<CommandInfo>.Comparer.Equals(this, other);
}