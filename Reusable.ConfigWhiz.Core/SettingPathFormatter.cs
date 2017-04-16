using System;
using System.Text;
using System.Text.RegularExpressions;
using Reusable.Extensions;
using Reusable.StringFormatting;

namespace Reusable.ConfigWhiz
{
    public class SettingPathFormatter : CustomFormatter
    {
        public static readonly SettingPathFormatter Instance = new SettingPathFormatter();

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

        public override string Format(string format, object arg, IFormatProvider formatProvider)
        {
            var match = Regex.Match(format, @"(?<full>[a-b])?((?<delimiter>.)?(?<short>[a-b])(?<strong>\[\])?)", RegexOptions.IgnoreCase);
            if (!match.Success || !(arg is SettingPath settingPath)) return null;

            var delimiter = match.Groups["delimiter"].Value;

            var name = new StringBuilder();

            if (match.Groups["full"].Success)
            {
                name
                    .Append(string.Join(delimiter, settingPath.ConsumerNamespace))
                    .Append(delimiter)
                    .Append(settingPath.ConsumerName)
                    .AppendWhen(
                        () => settingPath.InstanceName?.Trim('"') ?? string.Empty, 
                        instanceName => instanceName.IsNotNullOrEmpty(), 
                        (sb, instanceName) => sb.Append($"[\"{instanceName}\"]"))
                    .Append(delimiter);
            }

            name
                .Append(settingPath.ContainerName)
                .Append(delimiter)
                .Append(settingPath.SettingName);

            if (match.Groups["strong"].Success && settingPath.ElementName.IsNotNullOrEmpty())
            {
                name.Append($"[\"{settingPath.ElementName.Trim('"')}\"]");
            }

            return name.ToString();
        }
    }
}