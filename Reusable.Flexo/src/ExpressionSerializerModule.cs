using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using Autofac;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Reusable.Utilities.JsonNet;

namespace Reusable.Flexo
{
    public class ExpressionSerializerModule : Module
    {
        private readonly IEnumerable<Type> _expressionTypes;
        private readonly Action<JsonSerializer> _configureSerializer;

        public ExpressionSerializerModule(IEnumerable<Type> expressionTypes, Action<JsonSerializer> configureSerializer = default)
        {
            _expressionTypes = Expression.BuiltInTypes.Concat(expressionTypes);
            _configureSerializer = configureSerializer;
        }

        protected override void Load(ContainerBuilder builder)
        {
            foreach (var type in _expressionTypes)
            {
                builder.RegisterType(type);
            }

            builder.Register(ctx =>
            {
                var contractResolver = ctx.Resolve<IContractResolver>();
                var types = TypeDictionary.BuiltInTypes.AddRange(TypeDictionary.From(_expressionTypes));
                return new ExpressionSerializer(types, contractResolver, _configureSerializer);
            });
        }
    }
}