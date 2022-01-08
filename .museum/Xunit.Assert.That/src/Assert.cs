using System;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace Xunit
{
    public partial class Assert
    {
        [CanBeNull]
        public static Assert That => default;
    }
}