using System.ComponentModel;
using Reusable.Colin.Annotations;

namespace Reusable.Colin.Data
{
    public class HelpCommandParameter
    {
        [Parameter(Position = 1)]
        [CommandName("command-name", "cmd")]
        [Description("Display command usage.")]
        public string CommandName { get; set; }
    }


    //internal class ArgumentOrderComparer : IComparer<Argument>
    //{
    //    public int Compare(Argument x, Argument y)
    //    {
    //        var args = new[]
    //        {
    //            new {a = x, o = -1}, new {a = y, o = 1}
    //        };

    //        if (args.Any(z => z.a.Properties.HasPosition))
    //        {
    //            return args.All(z => z.a.Properties.HasPosition) ? x.Properties.Position - y.Properties.Position : args.First(z => z.a.Properties.HasPosition).o;
    //        }

    //        if (args.Any(z => z.a.Properties.IsRequired))
    //        {
    //            return args.All(z => z.a.Properties.IsRequired) ? string.Compare(x.Names.First(), y.Names.First(), StringComparison.OrdinalIgnoreCase) : args.First(z => z.a.Properties.IsRequired).o;
    //        }

    //        return string.Compare(x.Names.First(), y.Names.First(), StringComparison.OrdinalIgnoreCase);
    //    }
    //}
}