using System;
using System.IO;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Reusable;

internal static class Program
{
    [STAThread]
    private static async Task Main(string[] args)
    {
        //Examples.ConsoleColorizer();
        //Examples.SemanticExtensions();
        
        //Examples.Log();
        
        //await Experiments.ServicePipelineDemo.Test();
        await Experiments.ServicePipelineDemo2.Test();

        //Examples.Tokenize();
        //await Examples.SendEmailAsync_Mailr();
        //await Examples.SendEmailAsync_Smtp();
        //await Demo.SendEmailAsync_Smtp();
        //await Demo.SendEmailAsync_Mailr();

    }
}


internal static class StringBuilderExtensions
{
    public static bool Any(this StringBuilder stringBuilder) => stringBuilder.Length > 0;

    public static StringBuilder RemoveLast(this StringBuilder stringBuilder)
    {
        if (stringBuilder.Any())
        {
            stringBuilder.Length--;
        }

        return stringBuilder;
    }
}