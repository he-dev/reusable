using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Exceptionizer;
using Reusable.Reflection;

namespace Reusable.Flexo
{
    public abstract class Map<TFrom, TTo> : Expression
    {
        protected Map() : base(nameof(Map<TFrom, TTo>)) { }

        protected Map(string name) : base(name) { }

        public IEnumerable<Mapping<TFrom, TTo>> Mappings { get; set; }

        [JsonRequired]
        public IExpression Expression { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            var result = Expression.InvokeWithValidation(context);
            if (result is Constant<TFrom> constant)
            {
                var match =
                    Mappings.FirstOrDefault(mapping => mapping.From.Equals(constant.Value))
                    ?? throw DynamicException.Factory.CreateDynamicException($"MappingNotFound{nameof(Exception)}", $"Could not find mapping for '{constant.Value}'.");

                return Constant.FromValue(Name, match.To);
            }

            throw new InvalidExpressionException(typeof(Constant), result.GetType());
        }
    }

    public class Mapping<TFrom, TTo>
    {
        public TFrom From { get; set; }

        public TTo To { get; set; }
    }


//    public class BooleanToDouble : Map<bool, double>
//    {
//        public BooleanToDouble() : base(nameof(BooleanToDouble))
//        {
//            Mappings = ImmutableList.Create
//            (
//                new Mapping<bool, double> { From = true, To = Double.One },
//                new Mapping<bool, double> { From = false, To = Double.Zero }
//            );
//        }
//    }
}