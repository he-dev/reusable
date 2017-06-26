using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Reusable.ConfigWhiz.Paths;
using Reusable.Extensions;
using Reusable.StringFormatting;

namespace Reusable.ConfigWhiz
{
    public class IdentifierFormatter : CustomFormatter
    {
        public static readonly IdentifierFormatter Instance = new IdentifierFormatter();

        // https://regex101.com/r/OC6uiH/1
        /*

            short
                weak - .a
                strong - .a[]
            full
                weak - a.b
                strong - a.b[]

         */

        // Margins - a
        // Margins["0"] - a[]
        // App.Windows.MainWindow["Window1"].WindowDimensions.Margins - a.b
        // App.Windows.MainWindow["Window1"].WindowDimensions.Margins["0"] - a.b[]

        //public override string Format(string format, object arg, IFormatProvider formatProvider)
        //{
        //    var match = Regex.Match(format, @"(?<full>[a-b])?((?<delimiter>.)?(?<short>[a-b])(?<strong>\[\])?)", RegexOptions.IgnoreCase);
        //    if (!match.Success || !(arg is Identifier identifier)) return null;

        //    var delimiter = match.Groups["delimiter"].Value;

        //    var name = new StringBuilder();

        //    if (match.Groups["full"].Success)
        //    {
        //        name
        //            .Append(string.Join(delimiter, identifier.Context))
        //            .Append(delimiter)
        //            .Append(identifier.Consumer)
        //            .AppendWhen(
        //                () => identifier.Instance ?? string.Empty, 
        //                instanceName => instanceName.IsNotNullOrEmpty(), 
        //                (sb, instanceName) => sb.Append($"[\"{instanceName}\"]"))
        //            .Append(delimiter);
        //    }

        //    // Skip container name if it's the same as the consumer name.
        //    if (identifier.Container == identifier.Consumer)
        //    {
        //        name                    
        //            .Append(identifier.Setting);
        //    }
        //    else
        //    {
        //        name
        //            .Append(identifier.Container)
        //            .Append(delimiter)
        //            .Append(identifier.Setting);
        //    }

        //    if (match.Groups["strong"].Success && identifier.Element.IsNotNullOrEmpty())
        //    {
        //        name.Append($"[\"{identifier.Element}\"]");
        //    }

        //    return name.ToString();
        //}

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            var match = Regex.Match(format, @"(?<delimiter>.)(?<length>[a-z]+)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
            if (!match.Success || !(arg is Identifier identifier)) return null;

            var delimiter = match.Groups["delimiter"].Value;
            if (!Enum.TryParse<IdentifierLength>(match.Groups["length"].Value, true, out var length)) { return null; }

            var names = new List<string>();

            if (length == IdentifierLength.Unique && identifier.Context.Any())
            {
                names.AddRange(identifier.Context);
            }

            if (length >= IdentifierLength.Long && identifier.Consumer.IsNotNullOrEmpty())
            {
                var instance = identifier.Instance.IsNullOrEmpty() ? null : $"[\"{identifier.Instance}\"]";
                names.Add($"{identifier.Consumer}{instance}");
            }

            if (length >= IdentifierLength.Medium && identifier.Container.IsNotNullOrEmpty())
            {
                names.Add(identifier.Container);
            }

            // Skip container name if it's the same as the consumer name.
            if (length >= IdentifierLength.Short && identifier.Setting.IsNotNullOrEmpty())
            {
                var element = identifier.Element.IsNullOrEmpty() ? null : $"[\"{identifier.Element}\"]";
                names.Add($"{identifier.Setting}{element}");
            }            

            return string.Join(delimiter, names);
        }
    }
}