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
using Reusable.Commander;
using Reusable.Exceptionize;
using Reusable.IOnymous;
using Reusable.SmartConfig;
using Reusable.Extensions;
using Reusable.Flawless;
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
            var configuration = new Configuration<ITestConfig>(new CommandLineProvider(commandLine));
            var name = await configuration.GetItemAsync(x => x.Name);
        }

        [SettingType(Strength = SettingNameStrength.Low, Schema = CommandLineProvider.DefaultSchema)]
        internal interface ICommandLineSchema { }

        //[Schema(CommandLineProvider.DefaultSchema)]
        //[NameLength(SettingNameStrength.Low)]
        internal interface ITestConfig
        {
            string Name { get; }
        }
    }

    internal class CommandLineProvider : ResourceProvider
    {
        public const string DefaultSchema = "cmd";

        private readonly ICommandLine _commandLine;

        public CommandLineProvider(ICommandLine commandLine) : base(new SoftString[] { DefaultSchema }, ResourceMetadata.Empty)
        {
            _commandLine = commandLine;
        }

        // cmd://foo
        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata)
        {
            var id = uri.Path.Decoded;
            return
                _commandLine.Contains(id)
                    ? Task.FromResult<IResourceInfo>(new CommandLineInfo(uri, _commandLine[id]))
                    : Task.FromResult<IResourceInfo>(new CommandLineInfo(uri, default));
        }
    }

    internal class CommandLineInfo : ResourceInfo
    {
        private readonly IEnumerable<string> _values;

        public CommandLineInfo([NotNull] UriString uri, IEnumerable<string> values) : base(uri, MimeType.Binary)
        {
            _values = values;
        }

        public override bool Exists => !(_values is null);

        public override long? Length => _values?.Count();

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            using (var data = await ResourceHelper.SerializeAsBinaryAsync(_values))
            {
                await data.CopyToAsync(stream);
            }
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