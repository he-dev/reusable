using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Commander;
using Reusable.Exceptionize;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.OneTo1;
using Reusable.SmartConfig.Reflection;
using Xunit;

namespace Reusable.Tests.XUnit.Experimental
{
    /*
    public class UriSchemaAttribute:Attribute{}
    
    ResourcePrefix("test") // override
    ResourcePrefix(null) // disable
    // inherit by default
    ResourceScheme("setting")
    ResourceName("alias", Convention = TypeMember)
    ResourceProvider("alias")
     
      
     */

    public class MemberVisitorTools
    {
        [Fact]
        public async Task Blub()
        {
            var commandLine = new CommandLine
            {
                { "files", "first.txt" },
                { "files", "second.txt" },
                { "build", "debug" },
            };
            var configuration = new Configuration<ITestConfig>(new CommandLineArgumentProvider(commandLine));
            var actualFiles = await configuration.GetItemAsync(x => x.Files);
            var actualBuild = await configuration.GetItemAsync(x => x.Build);

            Assert.Equal(new[] { "first.txt", "second.txt" }, actualFiles);
            Assert.Equal("debug", actualBuild);
        }

        [ResourceName(Level = ResourceNameLevel.Member)]
        [CommandLineArgumentProviderScheme]
        internal interface ITestConfig
        {
            List<string> Files { get; }

            string Build { get; }
        }

        internal virtual async Task ExecuteAsync(IConfiguration<ITestConfig> parameter)
        {
            var files = await parameter.GetItemAsync(x => x.Files);
            //return Task.CompletedTask;
        }
    }

    

    public static class ext
    {
        public static IEnumerable<T> FindCustomAttributes<T>(this Type type) where T : Attribute
        {
            var queue = new Queue<Type>()
            {
                type
            };

            while (queue.Any())
            {
                type = queue.Dequeue();
                foreach (var a in type.GetCustomAttributes<T>())
                {
                    yield return a;
                }

                foreach (var i in type.GetInterfaces())
                {
                    queue.Enqueue(i);
                }
            }
        }
    }
}