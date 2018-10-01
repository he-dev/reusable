using System;
using System.Collections.Generic;
using System.ComponentModel;
using Reusable.Commander;
using Reusable.Commander.Annotations;

namespace Reusable.Tests.Commander.Integration
{    
    // Default values.
    internal class BagWithDefaultTypes : SimpleBag
    {
        public bool Bool1 { get; set; }

        [DefaultValue(false)]
        public bool Bool2 { get; set; }

        [DefaultValue(true)]
        public bool Bool3 { get; set; }

        public string String1 { get; set; }

        [DefaultValue("foo")]
        public string String2 { get; set; }

        public int Int1 { get; set; }

        public int? Int2 { get; set; }

        [DefaultValue(3)]
        public int Int3 { get; set; }

        public DateTime DateTime1 { get; set; }

        public DateTime? DateTime2 { get; set; }

        [DefaultValue("2018/01/01")]
        public DateTime DateTime3 { get; set; }

        public IList<int> List1 { get; set; }
    }    

    internal class Bag2 : SimpleBag
    {
        [DefaultValue(false)]
        public bool Property01 { get; set; }

        [DefaultValue(true)]
        public bool Property02 { get; set; }

        [Alias("p03")]
        [DefaultValue(false)]
        public bool Property03 { get; set; }

        [Alias("p04")]
        [DefaultValue("foo")]
        public string Property04 { get; set; }

        [Alias("p05")]
        public IList<int> Property05 { get; set; }
    }

#region Invalid bags

    // Contains duplicate parameters.
    internal class BagWithDuplicateParameter : SimpleBag
    {
        public string A { get; set; }

        [Alias("A")]
        public string B { get; set; }
    }

    // Contains invalid parameter positions.
    internal class BagWithInvalidParameterPosition : SimpleBag
    {
        [Position(1)]
        public string A { get; set; }

        [Position(3)]
        public string B { get; set; }
    }

    // Contains unsupported parameter type.
    internal class BagWithUnsupportedParameterType : SimpleBag
    {
        public AppDomain A { get; set; }
    }

#endregion
}