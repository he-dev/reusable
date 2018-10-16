using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.Extensions;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Reflection;

namespace Reusable.Tests.SmartConfig.Reflection
{
    using static Assert;

    [TestClass]
    public class SettingMetadataTest
    {
        private static readonly string Namespace = typeof(SettingMetadataTest).Namespace;

        private static Expression<Func<T>> CreateExpression<T>(Expression<Func<T>> expression) => expression;

        [TestMethod]
        public void FromExpression_CanGetMetadataFromBasicType()
        {
            var instance1 = new BasicType();
            var instance2 = new BasicType();

            var baseType = typeof(BasicType);

            // instance property as is
            {
                var instancePropertyExpressions = instance2.InstancePropertyExpressions().ToList();
                var baseInstanceProperty = typeof(BasicType).GetProperty(nameof(BasicType.InstanceProperty), BindingFlags.Instance | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(instancePropertyExpressions.Last());

                AreSame(instance2, actual.Instance);
                AreEqual(baseInstanceProperty, actual.Member);
                AreEqual(nameof(BasicType.InstanceProperty), actual.MemberName);

                AssertDefault(actual);
            }

            // static property as is
            {
                var staticPropertyExpressions = instance2.StaticPropertyExpressions().ToList();
                var baseStaticProperty = typeof(BasicType).GetProperty(nameof(BasicType.StaticProperty), BindingFlags.Static | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(staticPropertyExpressions.Last());

                IsNull(actual.Instance);
                AreEqual(baseStaticProperty, actual.Member);
                AreEqual(nameof(BasicType.StaticProperty), actual.MemberName);

                AssertDefault(actual);
            }

            // instance field as is
            {
                var instanceFieldExpressions = instance2.InstanceFieldExpressions().ToList();
                var baseInstanceField = typeof(BasicType).GetField("_instanceField", BindingFlags.Instance | BindingFlags.NonPublic);

                var actual = SettingMetadata.FromExpression(instanceFieldExpressions.Last(), nonPublic: true);

                AreSame(instance2, actual.Instance);
                AreEqual(baseInstanceField, actual.Member);
                AreEqual("_instanceField", actual.MemberName);

                AssertDefault(actual);
            }

            // static field as is
            {
                var staticFieldExpressions = instance2.StaticFieldExpressions().ToList();
                var baseStaticField = typeof(BasicType).GetField("_staticField", BindingFlags.Static | BindingFlags.NonPublic);

                var actual = SettingMetadata.FromExpression(staticFieldExpressions.Last(), nonPublic: true);

                IsNull(actual.Instance);
                AreEqual(baseStaticField, actual.Member);
                AreEqual("_staticField", actual.MemberName);

                AssertDefault(actual);
            }


            void AssertDefault(SettingMetadata actual)
            {
                // These cases are common to all tests.

                AreEqual(baseType, actual.Type);
                IsNull(actual.Prefix);
                AreEqual(Namespace, actual.Namespace);
                AreEqual(baseType.Name, actual.TypeName);
                IsTrue(actual.Validations.Empty());
                IsNull(actual.DefaultValue);
                IsNull(actual.ProviderName);
                AreEqual(SettingNameStrength.Inherit, actual.SettingNameStrength);
                AreEqual(PrefixHandling.Inherit, actual.PrefixHandling);
            }
        }

        [TestMethod]
        public void FromExpression_CanGetMetadataFromBasicSubtype()
        {
            var instance1 = new BasicSubtype();
            var instance2 = new BasicSubtype();

            var baseType = typeof(BasicType);
            var derivedType = typeof(BasicSubtype);

            // instance property of the subtype as is
            {
                var instancePropertyExpressions = instance2.InstancePropertyExpressions().ToList();
                var baseInstanceProperty = baseType.GetProperty(nameof(BasicType.InstanceProperty), BindingFlags.Instance | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(instancePropertyExpressions.Last());

                AreEqual(derivedType, actual.Type);
                AreSame(instance2, actual.Instance);
                AreEqual(derivedType.Name, actual.TypeName);
                AreEqual(baseInstanceProperty, actual.Member);
                AreEqual(nameof(BasicType.InstanceProperty), actual.MemberName);

                AssertDefault(actual);
            }

            // static property of the subtype as is
            {
                var staticPropertyExpressions = instance2.StaticPropertyExpressions().ToList();
                var baseStaticProperty = typeof(BasicType).GetProperty(nameof(BasicType.StaticProperty), BindingFlags.Static | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(staticPropertyExpressions.Last());

                AreEqual(baseType, actual.Type);
                IsNull(actual.Instance);
                AreEqual(baseType.Name, actual.TypeName);
                AreEqual(baseStaticProperty, actual.Member);
                AreEqual(nameof(BasicType.StaticProperty), actual.MemberName);

                AssertDefault(actual);
            }

            // instance field of the subtype as is
            {
                var instanceFieldExpressions = instance2.InstanceFieldExpressions().ToList();
                var baseInstanceField = typeof(BasicType).GetField("_instanceField", BindingFlags.Instance | BindingFlags.NonPublic);

                var actual = SettingMetadata.FromExpression(instanceFieldExpressions.Last(), nonPublic: true);

                AreSame(instance2, actual.Instance);
                AreEqual(baseInstanceField, actual.Member);
                AreEqual("_instanceField", actual.MemberName);

                AssertDefault(actual);
            }

            // static field of the subtype as is
            {
                var staticFieldExpressions = instance2.StaticFieldExpressions().ToList();
                var baseStaticField = typeof(BasicType).GetField("_staticField", BindingFlags.Static | BindingFlags.NonPublic);

                var actual = SettingMetadata.FromExpression(staticFieldExpressions.Last(), nonPublic: true);

                IsNull(actual.Instance);
                AreEqual(baseStaticField, actual.Member);
                AreEqual("_staticField", actual.MemberName);

                AssertDefault(actual);
            }


            void AssertDefault(SettingMetadata actual)
            {
                // These cases are common to all tests.

                IsNull(actual.Prefix);
                AreEqual(Namespace, actual.Namespace);

                IsTrue(actual.Validations.Empty());
                IsNull(actual.DefaultValue);
                IsNull(actual.ProviderName);
                AreEqual(SettingNameStrength.Inherit, actual.SettingNameStrength);
                AreEqual(PrefixHandling.Inherit, actual.PrefixHandling);
            }
        }

        [TestMethod]
        public void FromExpression_CanGetMetadataFromCustomizedType()
        {
            var instance1 = new CustomizedType();
            var instance2 = new CustomizedType();

            var baseType = typeof(CustomizedType);

            // instance property inherits type customization
            {
                var instancePropertyExpressions = instance2.InstanceProperty1Expressions().ToList();
                var baseInstanceProperty = baseType.GetProperty(nameof(CustomizedType.InstanceProperty1), BindingFlags.Instance | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(instancePropertyExpressions.Last());

                AreEqual(baseType, actual.Type);
                AreSame(instance2, actual.Instance);
                AreEqual(baseInstanceProperty, actual.Member);
                AreEqual("InstanceProperty1x", actual.MemberName);

                AssertDefault(actual);
            }

            // instance property overrides type customization
            {
                var instancePropertyExpressions = instance2.InstanceProperty2Expressions().ToList();
                var baseInstanceProperty = baseType.GetProperty(nameof(CustomizedType.InstanceProperty2), BindingFlags.Instance | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(instancePropertyExpressions.Last());

                AreEqual(baseType, actual.Type);
                AreSame(instance2, actual.Instance);
                AreEqual(baseInstanceProperty, actual.Member);
                AreEqual("InstanceProperty2x", actual.MemberName);

                //AssertDefault(actual);

                AreEqual("Prefix2", actual.Prefix);
                AreEqual("BaseClass2", actual.TypeName);
                AreEqual("Provider2", actual.ProviderName);
                AreEqual(SettingNameStrength.High, actual.SettingNameStrength);
                AreEqual(PrefixHandling.Enable, actual.PrefixHandling);
            }

            // static property inherits type customization
            {
                var staticPropertyExpressions = instance2.StaticPropertyExpressions().ToList();
                var baseStaticProperty = baseType.GetProperty(nameof(CustomizedType.StaticProperty1), BindingFlags.Static | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(staticPropertyExpressions.Last());

                IsNull(actual.Instance);
                AreEqual(baseStaticProperty, actual.Member);
                AreEqual("StaticProperty1x", actual.MemberName);

                AssertDefault(actual);
            }

            void AssertDefault(SettingMetadata actual)
            {
                // These cases are common to all tests.

                AreEqual(Namespace, actual.Namespace);
                IsTrue(actual.Validations.Empty());
                IsNull(actual.DefaultValue);
                AreEqual("Prefix1", actual.Prefix);
                AreEqual("BaseClass2", actual.TypeName);
                AreEqual("Provider1", actual.ProviderName);
                AreEqual(SettingNameStrength.Low, actual.SettingNameStrength);
                AreEqual(PrefixHandling.Enable, actual.PrefixHandling);
            }
        }

        [TestMethod]
        public void FromExpression_CanGetMetadataFromCustomizedSubtype()
        {
            var instance1 = new CustomizedSubtype();
            var instance2 = new CustomizedSubtype();

            var baseType = typeof(CustomizedSubtype);

            // instance property inherits type customization
            {
                var instancePropertyExpressions = instance2.InstanceProperty1Expressions().ToList();
                //var baseInstanceProperty = baseType.GetProperty(nameof(CustomizedSubtype.InstanceProperty1), BindingFlags.Instance | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(instancePropertyExpressions.Last());

                AreEqual(baseType, actual.Type);
                AreSame(instance2, actual.Instance);
                //AreEqual(baseInstanceProperty, actual.Member);
                AreEqual("InstanceProperty1xx", actual.MemberName);

                AreEqual("Prefix1", actual.Prefix);
                AreEqual("CustomizedType2", actual.TypeName);
                AreEqual("Provider1", actual.ProviderName);
                AreEqual(SettingNameStrength.Low, actual.SettingNameStrength);
                AreEqual(PrefixHandling.Enable, actual.PrefixHandling);
            }
        }

        #region Data

        // We need multiple expressions to ensure that it finds the correct closure.

        /// <summary>
        /// Without annotations.
        /// </summary>
        public class BasicType
        {
            private readonly string _instanceField;

            public string InstanceProperty { get; set; }

            private static readonly string _staticField;

            public static string StaticProperty { get; set; }

            public IEnumerable<LambdaExpression> InstancePropertyExpressions()
            {
                yield return CreateExpression(() => InstanceProperty);
                yield return CreateExpression(() => InstanceProperty);
            }

            public IEnumerable<LambdaExpression> InstanceFieldExpressions()
            {
                yield return CreateExpression(() => _instanceField);
                yield return CreateExpression(() => _instanceField);
            }

            public IEnumerable<LambdaExpression> StaticPropertyExpressions()
            {
                yield return CreateExpression(() => StaticProperty);
                yield return CreateExpression(() => StaticProperty);
            }

            public IEnumerable<LambdaExpression> StaticFieldExpressions()
            {
                yield return CreateExpression(() => _staticField);
                yield return CreateExpression(() => _staticField);
            }
        }

        public class BasicSubtype : BasicType
        {
        }

        [SettingType(
            Name = "CustomizedType2",
            Strength = SettingNameStrength.Low,
            PrefixHandling = PrefixHandling.Enable,
            ProviderName = "Provider1",
            Prefix = "Prefix1"
        )]
        public class CustomizedType
        {
            [SettingMember(Name = "InstanceProperty1x")]
            public virtual string InstanceProperty1 { get; set; }

            [SettingMember(
                Name = "InstanceProperty2x",
                Strength = SettingNameStrength.High,
                PrefixHandling = PrefixHandling.Disable,
                ProviderName = "Provider2",
                Prefix = "Prefix2"
            )]
            public virtual string InstanceProperty2 { get; set; }

            [SettingMember(Name = "StaticProperty1x")]
            public static string StaticProperty1 { get; set; }

            public virtual IEnumerable<LambdaExpression> InstanceProperty1Expressions()
            {
                yield return CreateExpression(() => InstanceProperty1);
                yield return CreateExpression(() => InstanceProperty1);
            }

            public IEnumerable<LambdaExpression> InstanceProperty2Expressions()
            {
                yield return CreateExpression(() => InstanceProperty2);
                yield return CreateExpression(() => InstanceProperty2);
            }

            public IEnumerable<LambdaExpression> StaticPropertyExpressions()
            {
                yield return CreateExpression(() => StaticProperty1);
                yield return CreateExpression(() => StaticProperty1);
            }
        }

        public class CustomizedSubtype : CustomizedType
        {
            [SettingMember(Name = "InstanceProperty1xx")]
            public override string InstanceProperty1 { get; set; }

            [SettingMember(Name = "InstanceProperty2xx", Strength = SettingNameStrength.High, PrefixHandling = PrefixHandling.Disable)]
            public override string InstanceProperty2 { get; set; }

            public override IEnumerable<LambdaExpression> InstanceProperty1Expressions()
            {
                yield return CreateExpression(() => InstanceProperty1);
                yield return CreateExpression(() => InstanceProperty1);
            }
        }

        #endregion
    }
}