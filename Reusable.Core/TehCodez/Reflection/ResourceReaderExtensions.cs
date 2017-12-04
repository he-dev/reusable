using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Reflection
{
    public static class ResourceReaderExtensions
    {
        [NotNull]
        public static Stream FindStream<T>([NotNull] this IResourceReader resources, [NotNull] Expression<Func<string, bool>> predicateExpression)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));
            if (predicateExpression == null) throw new ArgumentNullException(nameof(predicateExpression));

            return resources.GetStream(typeof(T).Assembly, resources.FindName<T>(predicateExpression));
        }

        [NotNull]
        public static string FindString<T>([NotNull] this IResourceReader resources, [NotNull] Expression<Func<string, bool>> predicateExpression)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));
            if (predicateExpression == null) throw new ArgumentNullException(nameof(predicateExpression));

            return resources.GetString(typeof(T).Assembly, resources.FindName<T>(predicateExpression));
        }

        [NotNull]
        public static string FindName<T>([NotNull] this IResourceReader resources, [NotNull] Expression<Func<string, bool>> predicateExpression)
        {
            if (resources == null) throw new ArgumentNullException(nameof(resources));
            if (predicateExpression == null) throw new ArgumentNullException(nameof(predicateExpression));

            var predicate = predicateExpression.Compile();

            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute - resources are never null
                return
                    resources
                        .GetResourceNames(typeof(T).Assembly)
                        .Where(predicate)
                        .Single2();
            }
            catch (EmptySequenceException innerException)
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"ResourceNotFound{nameof(Exception)}",
                    $"Expression {predicateExpression.ToString().QuoteWith("'")} does not match any resource in the {typeof(T).Assembly.GetName().Name.QuoteWith("'")} assembly.",
                    innerException);
            }
            catch (MoreThanOneElementException innerException)
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"MoreThanOneResourceFound{nameof(Exception)}",
                    $"Expression {predicateExpression.ToString().QuoteWith("'")} matches more then one resource in the {typeof(T).Assembly.GetName().Name.QuoteWith("'")} assembly.",
                    innerException);
            }
        }
    }
}