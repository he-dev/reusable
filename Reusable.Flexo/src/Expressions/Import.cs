using System;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Import : Expression
    {
        public Import() : base(default) { }

        [JsonRequired]
        public string Package { get; set; }

        protected override IConstant ComputeConstant(IImmutableContainer context)
        {
            if (string.IsNullOrEmpty(Package)) throw new InvalidOperationException($"{nameof(Package)} must be not null or empty.");

            return context.InvokePackage(Package);
        }
    }
}