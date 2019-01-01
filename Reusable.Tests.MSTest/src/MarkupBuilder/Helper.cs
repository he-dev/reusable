using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.IOnymous;

namespace Reusable.Tests.MarkupBuilder
{
    internal class Helper
    {
        public static IResourceProvider ResourceProvider { get; } = new RelativeProvider(new EmbeddedFileProvider(typeof(Helper).Assembly), @"res\MarkupBuilder");
    }
}
