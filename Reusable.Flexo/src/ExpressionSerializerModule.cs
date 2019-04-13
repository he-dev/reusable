using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Autofac;

namespace Reusable.Flexo
{
    public class ExpressionSerializerModule : Module
    {
        private readonly IEnumerable<Type> _expressionTypes;

        public ExpressionSerializerModule(IEnumerable<Type> expressionTypes)
        {
            _expressionTypes = expressionTypes;
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            foreach (var type in _expressionTypes)
            {
                builder.RegisterType(type);
            }                        
            
            builder
                .RegisterType<ExpressionSerializer>()
                .As<IExpressionSerializer>();
        }
    }
}