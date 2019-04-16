using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Reusable.IOnymous;
using Reusable.OneTo1;
using Reusable.Extensions;

namespace Reusable.SmartConfig
{
    using Token = SettingNameToken;
    using Level = SettingNameLevel;

    public class UriStringToSettingIdentifierConverter : TypeConverter<UriString, string>
    {
        protected override string ConvertCore(IConversionContext<UriString> context)
        {
            return new SettingIdentifier(context.Value);
        }
    }

    public class UriStringToSettingIdentifierConverter2 : TypeConverter<UriString, SettingIdentifier>
    {
        protected override SettingIdentifier ConvertCore(IConversionContext<UriString> context)
        {
            var uri = context.Value;
            
            // setting:///name/space/type/member?prefix=foo&instance=bar&providerName=baz&convention=TypeMember";

            //if (uri.IsRelative) throw new ArgumentException();
            //if (uri.Scheme != "setting") throw new ArgumentException();

            var names = uri.Path.Decoded.ToString().Split('/');
            var scope = names.Take(names.Length - 2).Join(".");
            var type = names.Skip(names.Length - 2).First();
            
            var query = uri.Query;
            
            var level = Enum.TryParse<SettingNameLevel>(query["level"].ToString(), ignoreCase: true, out var l) ? l : SettingNameLevel.TypeMember;

            return new SettingIdentifier
            (
                prefix: query.TryGetValue("prefix", out var prefix) ? prefix.ToString() : default,
                scope: level.In(Level.NamespaceTypeMember) ? scope : default,
                type: level.In(Level.NamespaceTypeMember, SettingNameLevel.TypeMember) ? type : default,
                member: names.Last(),
                handle: query.TryGetValue("handle", out var instance) ? instance.ToString() : default
            );
        }
    }
}