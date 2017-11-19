using System;
using System.Linq;
using System.Text.RegularExpressions;

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
    }
}