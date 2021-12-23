using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;

namespace Reusable.Essentials;

public interface IDynamicExceptionFactory
{
    /// <summary>
    /// Creates a dynamic exception. If the name does not end with 'Exception' then it's added automatically. In debug mode there is an Assert that will remind you of this.
    /// </summary>
    /// <param name="name">The name of the exception. If the name does not end with 'Exception' then it's added automatically. An Assert will remind you of this in DEBUG mode.</param>
    /// <param name="message">The message for the exception. It can be 'null' but you should provide it anyway if you want to find what wend wrong later</param>
    /// <param name="innerException">The inner exception. It can be 'null' but remember to set it if you have one.</param>
    [ContractAnnotation("name: null => halt")]
    Exception CreateDynamicException(ExceptionName name, string? message, Exception? innerException);

    /// <summary>
    /// Gets a dynamic exception type.
    /// </summary>
    /// <param name="name"></param>
    Type GetDynamicExceptionType(string name);
}

internal class DynamicExceptionFactory : IDynamicExceptionFactory
{
    private readonly ConcurrentDictionary<string, Type> _cache = new();

    public static IDynamicExceptionFactory Default { get; } = new DynamicExceptionFactory();

    public Exception CreateDynamicException(ExceptionName name, string? message, Exception? innerException)
    {
        var dynamicExceptionType = GetDynamicExceptionType(name);
        return (Exception)Activator.CreateInstance(dynamicExceptionType, message, innerException)!;
    }

    public Type GetDynamicExceptionType(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return _cache.GetOrAdd(name, CreateDynamicExceptionType);
    }

    private Type CreateDynamicExceptionType(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));

        Debug.Assert(name.EndsWith(nameof(Exception)), $"Exception name should end with '{nameof(Exception)}'.");

        var baseType = typeof(DynamicException);
        var baseConstructorParameterTypes = new[] { typeof(string), typeof(Exception) };
        var baseConstructor = baseType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, baseConstructorParameterTypes, null);

        var assemblyName = new AssemblyName($"DynamicAssembly_{Guid.NewGuid():N}");
#if NET47
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
#if NETCOREAPP2_2
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
        var typeBuilder = moduleBuilder.DefineType(name, TypeAttributes.Public);
        typeBuilder.SetParent(typeof(DynamicException));

        // Create a constructor with the same number of parameters as the base constructor.
        var constructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, baseConstructorParameterTypes);

        var ilGenerator = constructor.GetILGenerator();

        // Generate constructor code
        ilGenerator.Emit(OpCodes.Ldarg_0); // push 'this' onto stack.
        ilGenerator.Emit(OpCodes.Ldarg_1); // push 'message' onto stack.
        ilGenerator.Emit(OpCodes.Ldarg_2); // push 'innerException' onto stack.
        ilGenerator.Emit(OpCodes.Call, baseConstructor); // call base constructor

        ilGenerator.Emit(OpCodes.Nop); // C# compiler add 2 NOPS, so
        ilGenerator.Emit(OpCodes.Nop); // we'll add them, too.

        ilGenerator.Emit(OpCodes.Ret); // Return

        return typeBuilder.CreateType();
    }
}

public class ExceptionName
{
    private readonly string _name;

    private ExceptionName(string name)
    {
        if (name.IsNullOrEmpty()) throw new ArgumentNullException(nameof(name));

        _name =
            Regex.IsMatch(name, $"{nameof(Exception)}$", RegexOptions.IgnoreCase)
                ? name
                : $"{name}{nameof(Exception)}";
    }

    public override string ToString() => _name;

    public static implicit operator ExceptionName(string name) => new(name);

    public static implicit operator string(ExceptionName name) => name.ToString();
}