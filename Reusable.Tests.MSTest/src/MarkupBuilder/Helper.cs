using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.IO;

namespace Reusable.Tests.MarkupBuilder
{
    internal class Helper
    {
        public static IFileProvider ResourceProvider { get; } = new RelativeFileProvider(new EmbeddedFileProvider(typeof(Helper).Assembly), "res\\MarkupBuilder");
    }
}
