using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using Autofac;
using JetBrains.Annotations;
using Reusable.Commander.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OneTo1;
using Reusable.Reflection;
using TypeConverterAttribute = Reusable.OneTo1.TypeConverterAttribute;

namespace Reusable.Commander
{
    public interface ICommandParameterBinder
    {
        T Bind<T>(List<CommandLineArgument> args, object? context = default) where T : CommandParameter, new();
    }

    [UsedImplicitly]
    internal class CommandParameterBinder : ICommandParameterBinder
    {
        private readonly ILifetimeScope _scope;

        public CommandParameterBinder(ILifetimeScope scope) => _scope = scope;

        public T Bind<T>(List<CommandLineArgument> args, object? context = default) where T : CommandParameter, new()
        {
            var parameter = new T();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => !p.IsDefined(typeof(NotMappedAttribute)));

            foreach (var property in properties)
            {
                var argName = property.GetMultiName();
                var arg =
                    property.GetCustomAttribute<PositionAttribute>() is var position && position is {}
                        ? args.SingleOrNotFound(MultiName.Command) is var a && a && a.Count > position.Value ? new CommandLineArgument($"#{position}", a.ElementAt(position)) : CommandLineArgument.NotFound
                        : args.SingleOrNotFound(argName);

                var converter = 
                    property.GetCustomAttribute<TypeConverterAttribute>() is {} typeConverterAttribute
                        ? (ITypeConverter)Activator.CreateInstance(typeConverterAttribute.ConverterType)
                        : CommandArgumentConverter.Default;

                if (arg)
                {
                    var deserialize =
                        property.PropertyType.IsEnumerableOfT(except: typeof(string))
                            ? arg.AsEnumerable() as object
                            : arg.SingleOrDefault() as object;

                    var obj =
                        deserialize is {}
                            ? converter.Convert(deserialize, property.PropertyType)
                            : property.PropertyType == typeof(bool);

                    if (property.GetCustomAttributes<ValidationAttribute>() is var validations)
                    {
                        foreach (var validation in validations)
                        {
                            validation.Validate(obj, property.GetMultiName().Join(", ").EncloseWith("[]"));
                        }
                    }

                    property.SetValue(parameter, obj);
                }
                else
                {
                    if (property.GetCustomAttribute<RequiredAttribute>() is {})
                    {
                        throw DynamicException.Create("ArgumentNotFound", $"Could not bind required parameter '{argName.First()}' because there was no such argument in the command-line.");
                    }

                    if (property.GetCustomAttribute<ContextAttribute>() is {})
                    {
                        property.SetValue(parameter, context);
                    }

                    if (property.GetCustomAttribute<ServiceAttribute>() is {} service)
                    {
                        property.SetValue(parameter, _scope.Resolve(service.ServiceType ?? property.PropertyType));
                    }

                    if (property.GetCustomAttribute<DefaultValueAttribute>() is {} defaultValue)
                    {
                        property.SetValue(parameter, converter.Convert(defaultValue.Value, property.PropertyType));
                    }
                }
            }

            return parameter;
        }
    }

    internal static class CommandParameterBinderExtensions
    {
        public static CommandLineArgument SingleOrNotFound(this IEnumerable<CommandLineArgument> args, MultiName name)
        {
            return args.SingleOrDefault(a => a.Name.Equals(name)) ?? CommandLineArgument.NotFound;
        }
    }
}