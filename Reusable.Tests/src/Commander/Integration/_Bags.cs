using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reusable.Commander;
using Reusable.Commander.Annotations;
using Reusable.Data.Annotations;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Tests.Commander.Integration
{
    internal class BagWithoutAliases : SimpleBag
    {
        public string StringWithoutAlias { get; set; }
    }
    
    internal class BagWithAliases : SimpleBag
    {
        [Tags("swa")]
        public string StringWithAlias { get; set; }
    }

    internal class BagWithDefaultValues : SimpleBag
    {
        public bool BoolOnly { get; set; }

        [DefaultValue(false)]
        public bool BoolWithDefaultValue1 { get; set; }

        [DefaultValue(true)]
        public bool BoolWithDefaultValue2 { get; set; }

        public string StringOnly { get; set; }

        [DefaultValue("foo")]
        public string StringWithDefaultValue { get; set; }

        public int Int32Only { get; set; }

        public int? NullableInt32Only { get; set; }

        [DefaultValue(3)]
        public int Int32WithDefaultValue { get; set; }

        public DateTime DateTimeOnly { get; set; }

        public DateTime? NullableDateTime { get; set; }

        [DefaultValue("2018/01/01")]
        public DateTime DateTimeWithDefaultValue { get; set; }

        public IList<int> ListOnly { get; set; }
    }

    internal class BagWithMappedValues : SimpleBag
    {
        [DefaultValue(false)]
        public bool BoolWithDefaultValue1 { get; set; }

        [DefaultValue(true)]
        public bool BoolWithDefaultValue2 { get; set; }

        [Tags("p03")]
        [DefaultValue(false)]
        public bool Property03 { get; set; }

        [Tags("p04")]
        [DefaultValue("foo")]
        public string Property04 { get; set; }

        [Tags("p05")]
        public IList<int> Property05 { get; set; }
    }

    internal class BagWithRequiredValue : SimpleBag
    {
        [Required]
        public string RequiredString { get; set; }
    }

    internal class BagWithPositionalValues : SimpleBag
    {
        [Position(1)]
        public int Speed { get; set; }

        [Position(2)]
        public string Unit { get; set; }

        public bool IsMetric { get; set; }
    }

    #region Invalid bags

    // Contains duplicate parameters.
    internal class BagWithDuplicateParameter : SimpleBag
    {
        public string A { get; set; }

        [Tags("A")]
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