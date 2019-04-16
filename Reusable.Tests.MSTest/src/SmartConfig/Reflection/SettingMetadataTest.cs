using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.SmartConfig;
using Reusable.SmartConfig.Reflection;

namespace Reusable.Tests.MSTest.SmartConfig.Reflection
{
    [TestClass]
    public class SettingMetadataTest
    {
        private static readonly string Namespace = typeof(SettingMetadataTest).Namespace;

        private static Expression<Func<T>> CreateExpression<T>(Expression<Func<T>> expression) => expression;

        [TestMethod]
        public void FromExpression_CanGetMetadataFromBasicType()
        {
            var instance1 = new BaseType();
            var instance2 = new BaseType();

            var baseType = typeof(BaseType);

            // instance property as is
            {
                var instancePropertyExpressions = instance2.InstancePropertyExpressions().ToList();
                var baseInstanceProperty = typeof(BaseType).GetProperty(nameof(BaseType.InstanceProperty), BindingFlags.Instance | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(instancePropertyExpressions.Last());

                Assert.AreSame(instance2, actual.TypeInstance);
                Assert.AreEqual(baseInstanceProperty, actual.Member);
                Assert.AreEqual(nameof(BaseType.InstanceProperty), actual.MemberName);

                AssertDefault(actual);
            }

            // static property as is
            {
                var staticPropertyExpressions = instance2.StaticPropertyExpressions().ToList();
                var baseStaticProperty = typeof(BaseType).GetProperty(nameof(BaseType.StaticProperty), BindingFlags.Static | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(staticPropertyExpressions.Last());

                Assert.IsNull(actual.TypeInstance);
                Assert.AreEqual(baseStaticProperty, actual.Member);
                Assert.AreEqual(nameof(BaseType.StaticProperty), actual.MemberName);

                AssertDefault(actual);
            }

            // instance field as is
            {
                var instanceFieldExpressions = instance2.InstanceFieldExpressions().ToList();
                var baseInstanceField = typeof(BaseType).GetField("_instanceField", BindingFlags.Instance | BindingFlags.NonPublic);

                var actual = SettingMetadata.FromExpression(instanceFieldExpressions.Last(), nonPublic: true);

                Assert.AreSame(instance2, actual.TypeInstance);
                Assert.AreEqual(baseInstanceField, actual.Member);
                Assert.AreEqual("_instanceField", actual.MemberName);

                AssertDefault(actual);
            }

            // static field as is
            {
                var staticFieldExpressions = instance2.StaticFieldExpressions().ToList();
                var baseStaticField = typeof(BaseType).GetField("_staticField", BindingFlags.Static | BindingFlags.NonPublic);

                var actual = SettingMetadata.FromExpression(staticFieldExpressions.Last(), nonPublic: true);

                Assert.IsNull(actual.TypeInstance);
                Assert.AreEqual(baseStaticField, actual.Member);
                Assert.AreEqual("_staticField", actual.MemberName);

                AssertDefault(actual);
            }


            void AssertDefault(SettingMetadata actual)
            {
                // These cases are common to all tests.

                Assert.AreEqual(baseType, actual.Type);
                Assert.IsNull(actual.ResourcePrefix);
                Assert.AreEqual(Namespace, actual.Scope);
                Assert.AreEqual(baseType.Name, actual.TypeName);
                Assert.IsTrue(actual.Validations.Empty());
                Assert.IsNull(actual.DefaultValue);
                Assert.IsNull(actual.ResourceProviderName);
                //Assert.AreEqual(SettingNameStrength.Inherit, actual.Strength);
                //Assert.AreEqual(PrefixHandling.Inherit, actual.PrefixHandling);
            }
        }

        [TestMethod]
        public void FromExpression_CanGetMetadataFromBasicSubtype()
        {
            var subInstance1 = new SubType();
            var subInstance2 = new SubType();

            var baseType = typeof(BaseType);
            var subType = typeof(SubType);

            // instance property of the subtype as is
            {
                var instancePropertyExpressions = subInstance2.InstancePropertyExpressions().ToList();
                var baseInstanceProperty = baseType.GetProperty(nameof(BaseType.InstanceProperty), BindingFlags.Instance | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(instancePropertyExpressions.Last());

                Assert.AreEqual(subType, actual.Type);
                Assert.AreSame(subInstance2, actual.TypeInstance);
                Assert.AreEqual(subType.Name, actual.TypeName);
                Assert.AreEqual(baseInstanceProperty.ToString(), actual.Member.ToString());
                Assert.AreEqual(nameof(BaseType.InstanceProperty), actual.MemberName);

                AssertDefault(actual);
            }

            // static property of the subtype as is
            {
                var staticPropertyExpressions = subInstance2.StaticPropertyExpressions().ToList();
                var baseStaticProperty = typeof(BaseType).GetProperty(nameof(BaseType.StaticProperty), BindingFlags.Static | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(staticPropertyExpressions.Last());

                Assert.AreEqual(baseType, actual.Type);
                Assert.IsNull(actual.TypeInstance);
                Assert.AreEqual(baseType.Name, actual.TypeName);
                Assert.AreEqual(baseStaticProperty, actual.Member);
                Assert.AreEqual(nameof(BaseType.StaticProperty), actual.MemberName);

                AssertDefault(actual);
            }

            // non-public instance field of the subtype as is
//            {
//                var instanceFieldExpressions = subInstance2.InstanceFieldExpressions().ToList();
//                var baseInstanceField = typeof(BaseType).GetField("_instanceField", BindingFlags.Instance | BindingFlags.NonPublic);
//
//                var actual = SettingMetadata.FromExpression(instanceFieldExpressions.Last(), nonPublic: true);
//
//                Assert.AreSame(subInstance2, actual.TypeInstance);
//                Assert.AreEqual(baseInstanceField, actual.Member);
//                Assert.AreEqual("_instanceField", actual.MemberName);
//
//                AssertDefault(actual);
//            }

            // static field of the subtype as is
            {
                var staticFieldExpressions = subInstance2.StaticFieldExpressions().ToList();
                var baseStaticField = typeof(BaseType).GetField("_staticField", BindingFlags.Static | BindingFlags.NonPublic);

                var actual = SettingMetadata.FromExpression(staticFieldExpressions.Last(), nonPublic: true);

                Assert.IsNull(actual.TypeInstance);
                Assert.AreEqual(baseStaticField, actual.Member);
                Assert.AreEqual("_staticField", actual.MemberName);

                AssertDefault(actual);
            }


            void AssertDefault(SettingMetadata actual)
            {
                // These cases are common to all tests.

                Assert.IsNull(actual.ResourcePrefix);
                Assert.AreEqual(Namespace, actual.Scope);

                Assert.IsTrue(actual.Validations.Empty());
                Assert.IsNull(actual.DefaultValue);
                Assert.IsNull(actual.ResourceProviderName);
                //Assert.AreEqual(SettingNameStrength.Inherit, actual.Strength);
                //Assert.AreEqual(PrefixHandling.Inherit, actual.PrefixHandling);
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

                Assert.AreEqual(baseType, actual.Type);
                Assert.AreSame(instance2, actual.TypeInstance);
                Assert.AreEqual(baseInstanceProperty, actual.Member);
                Assert.AreEqual("InstanceProperty1x", actual.MemberName);

                AssertDefault(actual);
            }

            // instance property overrides type customization
            {
                var instancePropertyExpressions = instance2.InstanceProperty2Expressions().ToList();
                var baseInstanceProperty = baseType.GetProperty(nameof(CustomizedType.InstanceProperty2), BindingFlags.Instance | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(instancePropertyExpressions.Last());

                Assert.AreEqual(baseType, actual.Type);
                Assert.AreSame(instance2, actual.TypeInstance);
                Assert.AreEqual(baseInstanceProperty, actual.Member);
                Assert.AreEqual("InstanceProperty2x", actual.MemberName);

                //AssertDefault(actual);

                Assert.AreEqual("Prefix2", actual.ResourcePrefix);
                Assert.AreEqual("CustomizedType2", actual.TypeName);
                Assert.AreEqual("Provider2", actual.ResourceProviderName);
                //Assert.AreEqual(SettingNameStrength.High, actual.Strength);
                //Assert.AreEqual(PrefixHandling.Enable, actual.PrefixHandling);
            }

            // static property inherits type customization
            {
                var staticPropertyExpressions = instance2.StaticPropertyExpressions().ToList();
                var baseStaticProperty = baseType.GetProperty(nameof(CustomizedType.StaticProperty1), BindingFlags.Static | BindingFlags.Public);

                var actual = SettingMetadata.FromExpression(staticPropertyExpressions.Last());

                Assert.IsNull(actual.TypeInstance);
                Assert.AreEqual(baseStaticProperty, actual.Member);
                Assert.AreEqual("StaticProperty1x", actual.MemberName);

                AssertDefault(actual);
            }

            void AssertDefault(SettingMetadata actual)
            {
                // These cases are common to all tests.

                Assert.AreEqual(Namespace, actual.Scope);
                Assert.IsTrue(actual.Validations.Empty());
                Assert.IsNull(actual.DefaultValue);
                Assert.AreEqual("Prefix1", actual.ResourcePrefix);
                // moved to test
                //Assert.AreEqual("BaseClass2", actual.TypeName);
                Assert.AreEqual("Provider1", actual.ResourceProviderName);
                //Assert.AreEqual(SettingNameStrength.Low, actual.Strength);
                //Assert.AreEqual(PrefixHandling.Enable, actual.PrefixHandling);
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

                Assert.AreEqual(baseType, actual.Type);
                Assert.AreSame(instance2, actual.TypeInstance);
                //AreEqual(baseInstanceProperty, actual.Member);
                Assert.AreEqual("InstanceProperty1xx", actual.MemberName);

                Assert.AreEqual("Prefix1", actual.ResourcePrefix);
                Assert.AreEqual("CustomizedType2", actual.TypeName);
                Assert.AreEqual("Provider1", actual.ResourceProviderName);
                //Assert.AreEqual(SettingNameStrength.Low, actual.Strength);
                //Assert.AreEqual(PrefixHandling.Enable, actual.PrefixHandling);
            }
        }

        #region Data

        // We need multiple expressions to ensure that it finds the correct closure.

        /// <summary>
        /// Without annotations.
        /// </summary>
        public class BaseType
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

        public class SubType : BaseType
        {
        }

//        [SettingType(
//            Name = "CustomizedType2",
//            //Strength = SettingNameStrength.Low,
//            //PrefixHandling = PrefixHandling.Enable,
//            ProviderName = "Provider1",
//            Prefix = "Prefix1"
//        )]
        public class CustomizedType
        {
            //[SettingMember(Name = "InstanceProperty1x")]
            public virtual string InstanceProperty1 { get; set; }

//            [SettingMember(
//                Name = "InstanceProperty2x",
//                //Strength = SettingNameStrength.High,
//                //PrefixHandling = PrefixHandling.Disable,
//                ProviderName = "Provider2",
//                Prefix = "Prefix2"
//            )]
            public virtual string InstanceProperty2 { get; set; }

            //[SettingMember(Name = "StaticProperty1x")]
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
            [ResourceName("InstanceProperty1xx")]
            public override string InstanceProperty1 { get; set; }

            [ResourceName("InstanceProperty2xx", Level = ResourceNameLevel.NamespaceTypeMember)]
            [ResourcePrefix(null)]// th.High, PrefixHandling = PrefixHandling.Disable)]
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