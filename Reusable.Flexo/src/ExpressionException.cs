using System;
using System.Collections.Generic;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class ExpressionException : Exception
    {
        public ExpressionException(Exception inner) : base("An error occured while evaluating expression. See the inner exception for details.", inner) { }

        /// <summary>
        /// Gets or sets expression contexts that were available when the exception occured.
        /// </summary>
        public IList<IImmutableContainer> Contexts { get; set; }
    }
}