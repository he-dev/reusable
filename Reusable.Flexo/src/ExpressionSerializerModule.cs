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
using Reusable.Utilities.JsonNet.Abstractions;
using Reusable.Utilities.JsonNet.TypeDictionaries;

namespace Reusable.Flexo
{
    public class ExpressionSerializerModule : Module
    {
        private readonly ITypeDictionary _typeDictionary;
        private readonly Action<JsonSerializer>? _configureSerializer;

        public ExpressionSerializerModule(IEnumerable<Type> customTypes, Action<JsonSerializer>? configureSerializer = default)
        {
            _typeDictionary =
                BuildInTypeDictionary
                    .Default
                    .Add(new CustomTypeDictionary(Expression.BuiltInTypes))
                    .Add(new CustomTypeDictionary(customTypes));
            _configureSerializer = configureSerializer;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Register all types but the built-in ones.
            foreach (var x in _typeDictionary.Where(x => !x.Key.Contains(".")).Distinct())
            {
                builder.RegisterType(x.Value);
            }

            builder.Register(ctx => new ExpressionSerializer(_typeDictionary, ctx.Resolve<IContractResolver>(), _configureSerializer));
        }
    }
}