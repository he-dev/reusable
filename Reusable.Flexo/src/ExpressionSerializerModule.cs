using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Flexo.Abstractions;
using Reusable.Utilities.JsonNet;

namespace Reusable.Flexo
{
    public class ExpressionSerializerModule : Module
    {
        private readonly IImmutableDictionary<SoftString, Type> _expressionTypes;
        private readonly Action<JsonSerializer>? _configureSerializer;

        public ExpressionSerializerModule(IEnumerable<Type> expressionTypes, Action<JsonSerializer>? configureSerializer = default)
        {
            _expressionTypes =
                TypeDictionary
                    .BuiltInTypes
                    .AddRange(TypeDictionary.From(Expression.BuiltInTypes))
                    .AddRange(TypeDictionary.From(expressionTypes));
            _configureSerializer = configureSerializer;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Register all types but the built-in ones.
            foreach (var type in _expressionTypes.Except(TypeDictionary.BuiltInTypes, x => x.Value).Select(x => x.Value).Distinct())
            {
                builder.RegisterType(type);
            }

            builder.Register(ctx => new ExpressionSerializer(_expressionTypes, ctx.Resolve<IContractResolver>(), _configureSerializer));
        }
    }
}