using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.Configuration;

namespace Reusable.Apps.Server
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var isService = IsService(args);
            var contentRootPath = GetContentRootPath(isService);

            var configuration =
                new ConfigurationBuilder()
                    //.SetBasePath(contentRootPath)
                    .AddJsonFile("hosting.json", optional: true, reloadOnChange: true)
                    .AddJsonFile("appSettings.json", optional: true, reloadOnChange: true)
                    .Build();

            var host =
                WebHost.CreateDefaultBuilder(args)
                    .UseContentRoot(contentRootPath)
                    .UseConfiguration(configuration)
                    //.ConfigureAppConfiguration((context, builder) => { builder.AddJsonFile("hosting.json", optional: false, reloadOnChange: true); })
                    .UseStartup<Startup>()
                    //.UseApplicationInsights()
                    .Build();

            if (isService)
            {
                host.RunAsService();
            }
            else
            {
                host.Run();
            }
        }

        private static bool IsService(IEnumerable<string> args)
        {
            return !(Debugger.IsAttached || args.Contains("-console", StringComparer.OrdinalIgnoreCase));
        }

        private static string GetContentRootPath(bool isService)
        {
            return
                isService
                    ? Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)
                    : Directory.GetCurrentDirectory();
        }
    }
}
