using System.ComponentModel;
using Reusable.CommandLine.Annotations;

namespace Reusable.CommandLine.Tests.Data
{
    internal class TestParameter
    {
        [Parameter]
        public string Foo { get; set; }

        [Parameter(AllowShortName = false)]
        public int Bar { get; set; }

        [Parameter(AllowShortName = false)]
        [DefaultValue(1.5)]
        public double Baz { get; set; }

        [Parameter(AllowShortName = false)]
        public int[] Arr { get; set; }

        [Parameter(AllowShortName = false)]
        [DefaultValue(true)]
        public bool Flag1 { get; set; }

        [Parameter(AllowShortName = false)]
        [DefaultValue(true)]
        public bool Flag2 { get; set; }
    }
}