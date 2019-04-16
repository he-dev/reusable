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
            var configuration = new Configuration<ITestConfig>(new CommandLineProvider(new UriStringToSettingIdentifierConverter2(), commandLine));
            //var actualFiles = await configuration.GetItemAsync(x => x.Files);
            var actualBuild = await configuration.GetItemAsync(x => x.Build);
            
            //Assert.Equal(new[] { "first.txt", "second.txt" }, actualFiles);
            Assert.Equal("debug", actualBuild);
        }

        //[Schema(CommandLineProvider.DefaultSchema)]
        //[NameLength(SettingNameStrength.Low)]
        [ResourceName(Level = ResourceNameLevel.Member)]
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

    internal class CommandLineProvider : ResourceProvider
    {
        public const string DefaultSchema = "setting";

        private readonly ITypeConverter _uriToIdentifierConverter;
        private readonly ICommandLine _commandLine;

        public CommandLineProvider(ITypeConverter uriToIdentifierConverter, ICommandLine commandLine) : base(new SoftString[] { DefaultSchema }, ResourceMetadata.Empty)
        {
            _uriToIdentifierConverter = uriToIdentifierConverter;
            _commandLine = commandLine;
            // todo - validate required parameters
        }

        // cmd://foo
        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata)
        {
            var identifier = (Identifier)(SoftString)(SettingIdentifier)_uriToIdentifierConverter.Convert(uri, typeof(SettingIdentifier));
            var values = _commandLine[identifier];
            var data = uri.Query.TryGetValue("isCollection", out var ic) && bool.TryParse(ic.ToString(), out var icb) && icb ? (object)values : values.SingleOrDefault();

            return
                _commandLine.Contains(identifier)
                    ? Task.FromResult<IResourceInfo>(new CommandLineInfo(uri, data))
                    : Task.FromResult<IResourceInfo>(new CommandLineInfo(uri, default));
        }
    }

    internal class CommandLineInfo : ResourceInfo
    {
        private readonly object _value;

        public CommandLineInfo([NotNull] UriString uri, object value) : base(uri, MimeType.Json)
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long? Length => null; //?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            using (var data = await ResourceHelper.SerializeAsJsonAsync(_value))
            {
                await data.Rewind().CopyToAsync(stream);
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