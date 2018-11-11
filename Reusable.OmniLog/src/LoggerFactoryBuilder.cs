using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Reusable.Collections;
using Reusable.OmniLog.Collections;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.Serialization;
using Reusable.Validation;

namespace Reusable.OmniLog.NewBuilder
{
    public static class LoggerFactoryExtensions
    {
        private static readonly IBouncer<LoggerFactory> SelfBouncer = Bouncer.For<LoggerFactory>(builder =>
        {
            builder.Ensure(x => x.Observers.Any()).WithMessage("You need to add at least one observer.");
            //builder.Block(x => string.IsNullOrEmpty(x._environment)).WithMessage("You need to specify the environment.");
            //builder.Block(x => string.IsNullOrEmpty(x._product)).WithMessage("You need to specif the product.");
        });

        
    }
}