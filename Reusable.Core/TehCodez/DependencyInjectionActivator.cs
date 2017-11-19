using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable
{
    public interface IDependencyInjectionActivator
    {
        object CreateInstance(Type type, params object[] candidates);
    }

    public class DependencyInjectionActivator : IDependencyInjectionActivator
    {
        private static readonly IEqualityComparer<Type> TypeInheritanceComparer = 
            RelayEqualityComparer<Type>
                .CreateWithoutHashCode((candidate, parameter) => parameter.IsAssignableFrom(candidate));

        public object CreateInstance(Type type, params object[] candidates)
        {
            var constructor = type.GetConstructors().Single();

            var parameters =
                constructor
                    .GetParameters()
                    .Join(
                        candidates,
                        parameter => parameter.ParameterType,
                        candidate => candidate.GetType(),
                        (parameter, candidate) => candidate,
                        TypeInheritanceComparer
                    ).ToArray();

            var dependenciesResolved = (parameters.Length == constructor.GetParameters().Length);
            if (dependenciesResolved)
            {
                return Activator.CreateInstance(type, parameters);
            }

            var missingDependencies =
                constructor
                    .GetParameters()
                    .Select(p => p.ParameterType)
                    .Except(parameters.Select(p => p.GetType()), TypeInheritanceComparer)
                    .ToList();

            var message =
                $"Some dependecies for {type.ToPrettyString().QuoteWith("'")} could not be resolved: " +
                $"{missingDependencies.Select(t => t.ToPrettyString()).Join(", ").EncloseWith("[]")}";
            throw DynamicException.Factory.CreateDynamicException($"MissingDependency{nameof(Exception)}", message, null);
        }
    }
}