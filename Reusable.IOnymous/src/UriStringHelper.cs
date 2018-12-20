using System;
using System.Globalization;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;

namespace Reusable.IOnymous
{
    public static class UriStringHelper
    {
        private static readonly string DecodeCharacters = "!#$&%'()*+,/:;=?@[]";
        
        private static readonly string DecodePattern = $"%(?<hex>{DecodeCharacters.Select(c => $"{(int)c:X2}").Join("|")})";

        /// <summary>
        /// Encodes the specified value by replacing reserved characters with their byte codes. '%' is always handled.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="reservedCharacters">If not specified then only '%' gets encoded.</param>
        public static string Encode(string value, string reservedCharacters = null)
        {
            reservedCharacters = reservedCharacters ?? string.Empty;

            if (reservedCharacters.Contains('%'))
            {
                throw new ArgumentOutOfRangeException
                (
                    paramName:nameof(reservedCharacters), 
                    message: "You cannot encode '%' because it's always encoded automatically."
                );
            }
            
            var escaped = 
                reservedCharacters
                    .Select(c => Regex.Escape(c.ToString()))
                    // %25 = % - is a special case that's automatically encoded and only if it's actually not encoded yet.
                    .Append("%(?!25)");
        
            var encodePattern = $"(?<reserved>{escaped.Join("|")})";
            return Regex.Replace(value, encodePattern, m => EncodeCharacter(m.Groups["reserved"].Value[0]));

            string EncodeCharacter(char c) => $"%{(int)c:X2}";
        }

        public static string Decode(string value)
        {
            return Regex.Replace(value, DecodePattern, m => DecodeCharacter(m.Groups["hex"].Value));

            string DecodeCharacter(string hex) => ((char)int.Parse(hex, NumberStyles.HexNumber)).ToString();
        }

        /// <summary>
        /// Replaces all '\' with '/' 
        /// </summary>
        public static string Normalize(string uri) => Regex.Replace(uri, @"\\", "/");
    }
}