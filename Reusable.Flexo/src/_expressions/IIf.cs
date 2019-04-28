﻿using System;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    // ReSharper disable once InconsistentNaming - we want this name!
    public class IIf : Expression<object>, IExtension<bool>
    {
        public IIf(ILogger<IIf> logger) : base(logger, nameof(IIf)) { }        
        
        //public IIf() : base(nameof(IIf)) { }

        [JsonRequired]
        [This]
        public IExpression Predicate { get; set; }

        public IExpression True { get; set; }

        public IExpression False { get; set; }

        protected override Constant<object> InvokeCore(IImmutableSession context)
        {
            if (True is null && False is null) throw new InvalidOperationException($"You need to specify at least one result ({nameof(True)}/{nameof(False)}).");
            
            var @this = context.PopThis().Invoke(context);
            

//            if (context.TryPopExtensionInput(out bool input))
//            {
//                if (input)
//                {
//                    var trueResult = True?.Invoke(context);
//                    return (Name, trueResult?.Value, trueResult?.Context);
//                }
//                else
//                {
//                    var falseResult = False?.Invoke(context);
//                    return (Name, falseResult?.Value, falseResult?.Context);
//                }
//            }
//            else
            {
                //var result = Predicate.Invoke(context);

                if (@this.Value<bool>())
                {
                    var trueResult = True?.Invoke(@this.Context);
                    return (Name, trueResult?.Value, trueResult?.Context);
                }
                else
                {
                    var falseResult = False?.Invoke(@this.Context);
                    return (Name, falseResult?.Value, falseResult?.Context);
                }
            }
        }
    }
}