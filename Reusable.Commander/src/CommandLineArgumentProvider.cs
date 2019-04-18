using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.SmartConfig;

namespace Reusable.Commander
{
    public class CommandLineArgumentProviderSchemeAttribute : ResourceSchemeAttribute
    {
        public CommandLineArgumentProviderSchemeAttribute() : base(CommandLineArgumentProvider.DefaultScheme) { }
    }

    public class CommandLineArgumentProvider : ResourceProvider
    {
        public new const string DefaultScheme = "setting";

        private readonly ITypeConverter _uriToIdentifierConverter;
        private readonly ICommandLine _commandLine;

        public CommandLineArgumentProvider(ITypeConverter<UriString, string> uriToIdentifierConverter, ICommandLine commandLine)
            : base(new SoftString[] { DefaultScheme }, IOnymous.Metadata.Empty)
        {
            _uriToIdentifierConverter = uriToIdentifierConverter;
            _commandLine = commandLine;
            // todo - validate required parameters
        }

        public CommandLineArgumentProvider(ICommandLine commandLine)
            : this(new UriStringToSettingIdentifierConverter(), commandLine) { }

        // cmd:///foo
        protected override Task<IResourceInfo> GetAsyncInternal(UriString uri, Metadata metadata)
        {
            var identifier = (Identifier)(SoftString)_uriToIdentifierConverter.Convert<SettingIdentifier>(uri);

            if (!_commandLine.Contains(identifier))
            {
                Task.FromResult<IResourceInfo>(new CommandLineArgumentInfo(uri, default));
            }

            var values = _commandLine[identifier];
            var isCollection = uri.Query.TryGetValue(SettingQueryStringKeys.IsCollection, out var ic) && bool.TryParse(ic.ToString(), out var icb) && icb;
            var data = isCollection ? (object)values : values.SingleOrDefault();

            return Task.FromResult<IResourceInfo>(new CommandLineArgumentInfo(uri, data));
        }
    }

    public class CommandLineArgumentInfo : ResourceInfo
    {
        private readonly object _value;

        public CommandLineArgumentInfo([NotNull] UriString uri, object value) : base(uri, Metadata.Empty.Format(MimeType.Json))
        {
            _value = value;
        }

        public override bool Exists => !(_value is null);

        public override long? Length => null; //?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            // It's easier to handle arguments as json because it takes care of all conversions.
            using (var data = await ResourceHelper.SerializeAsJsonAsync(_value))
            {
                await data.Rewind().CopyToAsync(stream);
            }
        }
    }
}