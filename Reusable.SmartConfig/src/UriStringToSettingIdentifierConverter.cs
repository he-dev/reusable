using System;
using System.Linq;
using System.Linq.Custom;
using Reusable.IOnymous;
using Reusable.OneTo1;

namespace Reusable.SmartConfig
{
    using Token = SettingNameToken;
    using Level = ResourceNameLevel;
    
    public class UriStringToSettingIdentifierConverter : TypeConverter<UriString, SettingIdentifier>
    {
        protected override SettingIdentifier ConvertCore(IConversionContext<UriString> context)
        {
            var uri = context.Value;

            // setting:///name/space/type/member?prefix=foo&instance=bar&providerName=baz&convention=TypeMember";

            var names = uri.Path.Decoded.ToString().Split('/');
            var scope = names.Take(names.Length - 2).Join(".");
            var type = names.Skip(names.Length - 2).First();

            var query = uri.Query;

            var level = Enum.TryParse<Level>(query[SettingQueryStringKeys.Level].ToString(), ignoreCase: true, out var l) ? l : Level.TypeMember;

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