using System;
using System.Linq;
using System.Reflection;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Xunit;

namespace Reusable.Utilities.XUnit.Annotations
{
    //[DataDiscoverer("Xunit.Sdk.MemberDataDiscoverer", "xunit.core")]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class SmartMemberDataAttribute : MemberDataAttributeBase
    {
        public SmartMemberDataAttribute(string memberName, params object[] parameters) : base(memberName, parameters) { }

        protected override object[] ConvertDataItem(MethodInfo testMethod, object item)
        {
            try
            {
                return CreateDataItem(testMethod, item);
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    $"DataItemConversion",
                    $"Could not convert '{item.GetType().ToPrettyString()}' for '{GetTestMethodInfo()}'. See the inner exception for details.",
                    inner
                );
            }

            // Creates text: MyTest.TestMethod
            string GetTestMethodInfo() => $"{testMethod.DeclaringType.ToPrettyString()}.{testMethod.Name}";
        }

        private static object[] CreateDataItem(MethodInfo testMethod, object item)
        {
            var itemProperties = item.GetType().GetProperties().ToDictionary(p => p.Name, p => p, SoftString.Comparer);
            var testMethodParameters = testMethod.GetParameters();
            var dataItem = new object[testMethodParameters.Length];

            // We need the index to set the correct item in the result array.
            foreach (var (testMethodParameter, i) in testMethodParameters.Select((x, i) => (x, i)))
            {
                if (itemProperties.TryGetValue(testMethodParameter.Name, out var itemProperty))
                {
                    if (testMethodParameter.ParameterType.IsAssignableFrom(itemProperty.PropertyType))
                    {
                        dataItem[i] = itemProperty.GetValue(item);
                    }
                    else
                    {
                        throw DynamicException.Create
                        (
                            $"ParameterTypeMismatch",
                            $"Cannot assign value of type '{itemProperty.PropertyType.ToPrettyString()}' " +
                            $"to the parameter '{testMethodParameter.Name}' of type '{testMethodParameter.ParameterType.ToPrettyString()}'."
                        );
                    }
                }
                else
                {
                    if (testMethodParameter.IsOptional)
                    {
                        dataItem[i] = testMethodParameter.DefaultValue;
                    }
                    else
                    {
                        throw DynamicException.Create
                        (
                            $"ParameterNotOptional",
                            $"Data item does not specify the required parameter '{testMethodParameter.Name}'."
                        );
                    }
                }
            }

            return dataItem;
        }
    }
}