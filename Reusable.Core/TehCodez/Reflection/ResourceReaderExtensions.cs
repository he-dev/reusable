using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Reflection
{
    [PublicAPI]
    public static class ResourceReaderExtensions
    {
        /// <summary>
        /// Gets resource names from the calling assembly.
        /// </summary>
        [NotNull, ItemNotNull]
        public static IEnumerable<string> GetResourceNames([NotNull] this IResourceReader reader)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return reader.GetResourceNames(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Finds a stream in the calling assembly.
        /// </summary>
        [NotNull]
        public static Stream FindStream([NotNull] this IResourceReader reader, [NotNull] Expression<Func<string, bool>> predicateExpression)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (predicateExpression == null) throw new ArgumentNullException(nameof(predicateExpression));

            return reader.FindStream(predicateExpression, Assembly.GetCallingAssembly());
        }
        
        [NotNull]
        public static Stream FindStream([NotNull] this IResourceReader reader, [NotNull] Expression<Func<string, bool>> predicateExpression, [NotNull] Assembly assembly)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (predicateExpression == null) throw new ArgumentNullException(nameof(predicateExpression));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            return reader.GetStream(reader.FindName(predicateExpression, assembly), assembly);
        }

        /// <summary>
        /// Finds a string in the calling assembly.
        /// </summary>
        [NotNull]
        public static string FindString([NotNull] this IResourceReader reader, [NotNull] Expression<Func<string, bool>> predicateExpression)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (predicateExpression == null) throw new ArgumentNullException(nameof(predicateExpression));

            return reader.FindString(predicateExpression, Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// Finds a string in the calling assembly.
        /// </summary>
        [NotNull]
        public static string FindString([NotNull] this IResourceReader reader, [NotNull] string name)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (name == null) throw new ArgumentNullException(nameof(name));

            return reader.FindString(current => current.EndsWith(name, StringComparison.OrdinalIgnoreCase), Assembly.GetCallingAssembly());
        }

        [NotNull]
        public static string FindString([NotNull] this IResourceReader reader, [NotNull] Expression<Func<string, bool>> predicateExpression, [NotNull] Assembly assembly)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (predicateExpression == null) throw new ArgumentNullException(nameof(predicateExpression));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            return reader.GetString(reader.FindName(predicateExpression, assembly), assembly);
        }

        /// <summary>
        /// Finds a resource name in the calling assembly.
        /// </summary>
        [NotNull]
        public static string FindName([NotNull] this IResourceReader reader, [NotNull] Expression<Func<string, bool>> predicateExpression)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (predicateExpression == null) throw new ArgumentNullException(nameof(predicateExpression));

            return reader.FindName(predicateExpression, Assembly.GetCallingAssembly());
        }

        [NotNull]
        public static string FindName([NotNull] this IResourceReader reader, [NotNull] Expression<Func<string, bool>> predicateExpression, [NotNull] Assembly assembly)
        {
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            if (predicateExpression == null) throw new ArgumentNullException(nameof(predicateExpression));
            if (assembly == null) throw new ArgumentNullException(nameof(assembly));

            var predicate = predicateExpression.Compile();

            try
            {
                // ReSharper disable once AssignNullToNotNullAttribute - resources are never null
                return
                    reader
                        .GetResourceNames(assembly)
                        .Where(predicate)
                        .Single2();
            }
            catch (EmptySequenceException innerException)
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"ResourceNotFound{nameof(Exception)}",
                    $"Expression {predicateExpression.ToString().QuoteWith("'")} does not match any resource in the {assembly.GetName().Name.QuoteWith("'")} assembly.",
                    innerException);
            }
            catch (MoreThanOneElementException innerException)
            {
                throw DynamicException.Factory.CreateDynamicException(
                    $"MoreThanOneResourceFound{nameof(Exception)}",
                    $"Expression {predicateExpression.ToString().QuoteWith("'")} matches more then one resource in the {assembly.GetName().Name.QuoteWith("'")} assembly.",
                    innerException);
            }
        }
    }
}