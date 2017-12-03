using System;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable
{
    public static class StringExtensions
    {
        public static string ToExceptionName(this string sentence)
        {
            sentence = sentence.Trim();

            var isSentence = sentence.IndexOf(' ') > 0;
            if (!isSentence)
            {
                return sentence;
            }

            var words = sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var capitalizedWords =
                from word in words
                select Regex.Replace(word.ToLower(), "^[a-z]", m => m.Value.ToUpper());

            return string.Join(string.Empty, capitalizedWords);
        }

        [NotNull, ContractAnnotation("typeFullName: null => halt; notnull => notnull")]
        public static string ToShortName([NotNull] this string typeFullName)
        {
            if (typeFullName == null) throw new ArgumentNullException(nameof(typeFullName));

            // https://regex101.com/r/p5FY2y/1
            return 
                Regex
                    .Replace(
                        typeFullName, 
                        @"(?:(?:\.?(?<type>[a-z]+)))+", 
                        m => m.Groups["type"].Value, 
                        RegexOptions.IgnoreCase
                    );
        }
    }
}