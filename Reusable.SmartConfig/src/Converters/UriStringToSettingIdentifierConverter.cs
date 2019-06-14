using System;
using System.Linq;
using System.Linq.Custom;
using System.Text;
using Reusable.IOnymous;
using Reusable.OneTo1;

namespace Reusable.SmartConfig
{
    using Token = SettingNameToken;
    using Level = ResourceNameLevel;
    
    public class UriStringToSettingIdentifierConverter : TypeConverter<UriString, string>
    {
        protected override string ConvertCore(IConversionContext<UriString> context)
        {
            var uri = context.Value;

            var nameBytes = System.Convert.FromBase64String(uri.Query["name"].ToString());
            var name = Encoding.UTF8.GetString(nameBytes);

            // setting:///name/space/type/member?prefix=foo&instance=bar&providerName=baz&convention=TypeMember";

            var names = uri.Path.Decoded.ToString().Split('/');
            var scope = names.Take(names.Length - 2).Join(".");
            var type = names.Skip(names.Length - 2).First();

            var query = uri.Query;

            var levelString = query.TryGetValue(SettingQueryStringKeys.Level, out var ls) ? ls.ToString() : default;
            var level = Enum.TryParse<Level>(levelString, ignoreCase: true, out var l) ? l : Level.TypeMember;

            return new SettingIdentifier
            (
                prefix: query.TryGetValue(SettingQueryStringKeys.Prefix, out var prefix) ? prefix.ToString() : default,
                scope: level.In(Level.NamespaceTypeMember) ? scope : default,
                type: level.In(Level.NamespaceTypeMember, Level.TypeMember) ? type : default,
                member: names.Last(),
                handle: query.TryGetValue(SettingQueryStringKeys.Handle, out var instance) ? instance.ToString() : default
            );
        }
    }
}