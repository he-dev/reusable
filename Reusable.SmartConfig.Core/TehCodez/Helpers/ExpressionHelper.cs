using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.SmartConfig.Annotations;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig.Helpers
{
    public static class ExpressionHelper
    {
        [NotNull]
        public static SettingName GetSettingName([NotNull] this LambdaExpression lambdaExpression, [CanBeNull] string instance = null)
        {
            var smartSettingAttribute = lambdaExpression.GetCustomAttribute<SmartSettingAttribute>();

            var hasCustomName = !string.IsNullOrEmpty(smartSettingAttribute?.Name);
            if (hasCustomName)
            {
                return new SettingName(smartSettingAttribute.Name);
            }

            var classType = ClassTypeFinder.FindClassType(lambdaExpression, true) ?? throw ("UnsupportedSettingExpressionException", "Member's declaring type could not be determined.").ToDynamicException(); ;

            return new SettingName(lambdaExpression.MemberExpression().Member.Name)
            {
                Namespace = classType.Namespace,
                Type = classType.Name,
                Instance = instance
            };
        }

        [CanBeNull]
        public static T GetCustomAttribute<T>([NotNull] this LambdaExpression lambdaExpression) where T : Attribute
        {
            return lambdaExpression.MemberExpression().Member.GetCustomAttribute<T>();
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<T> GetCustomAttributes<T>([NotNull] this LambdaExpression lambdaExpression) where T : Attribute
        {
            return lambdaExpression.MemberExpression().Member.GetCustomAttributes<T>();
        }

        [CanBeNull]
        internal static object GetValue([NotNull] this LambdaExpression lambdaExpression)
        {
            if (lambdaExpression == null)
            {
                throw new ArgumentNullException(nameof(lambdaExpression));
            }

            var memberExpression = lambdaExpression.MemberExpression();

            var obj = ObjectFinder.FindObject(lambdaExpression);

            switch (memberExpression.Member.MemberType)
            {
                case MemberTypes.Property:
                    var property = (PropertyInfo)memberExpression.Member;
                    return property.GetValue(obj);

                case MemberTypes.Field:
                    return ((FieldInfo)memberExpression.Member).GetValue(obj);

                default:
                    throw new ArgumentException($"Member must be either a {nameof(MemberTypes.Property)} or a {nameof(MemberTypes.Field)}.");
            }
        }

        // Gets the object this member belongs to or null if it's in a static class.
        //[CanBeNull]
        //public static object GetObject(this MemberExpression memberExpression)
        //{
        //    switch (memberExpression.Expression)
        //    {
        //        case null:
        //            // This is a static class.
        //            return null;

        //        case MemberExpression anonymousMemberExpression:
        //            // Extract constant value from the anonyous-wrapper
        //            var container = ((ConstantExpression)anonymousMemberExpression.Expression).Value;
        //            return ((FieldInfo)anonymousMemberExpression.Member).GetValue(container);

        //        case ConstantExpression constantExpression:
        //            return constantExpression.Value;

        //        //case ParameterExpression parameterExpression:
        //        //    return parameterExpression;
        //        default:
        //            throw new ArgumentException($"Expression '{memberExpression.Expression.GetType().Name}' is not supported.");
        //    }
        //}

        public static MemberExpression MemberExpression(this LambdaExpression lambdaExpression)
        {
            if (lambdaExpression.Body is MemberExpression memberExpression)
            {
                return memberExpression;
            }

            throw new ArgumentException(paramName: nameof(lambdaExpression), message: "The body of the lambda expression must be a member expression.");
        }
    }

    

    //    // Derived type used in construtor.
    //    Type GetDerivedTypeInsideClass()
    //    {
    //    return memberExpr.Expression is ConstantExpression constExpr ? constExpr.Type : null;
    //}

    //// Derived type used via instance.
    //Type GetDerivedTypeFromInstance()
    //{
    //return
    //memberExpr.Expression is MemberExpression memberExpr2 &&
    //memberExpr2.Expression is ConstantExpression constExpr ? constExpr.Value?.GetType()?.GetFields()?[0]?.FieldType : null;
    //}

    //Type GetDefaultType()
    //{
    //return memberExpr.Member.DeclaringType;
    //}

    //var type =
    //    GetDerivedTypeInsideClass() ??
    //    GetDerivedTypeFromInstance() ??
    //    GetDefaultType() ?? 
    //    throw ("UnsupportedSettingPathException", "Member's declaring type could not be determined.").ToDynamicException();

}