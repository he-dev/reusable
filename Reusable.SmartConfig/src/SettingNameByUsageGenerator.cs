using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.SmartConfig.Data;

namespace Reusable.SmartConfig
{
    /*

    Setting names are ordered by the usage frequency.

    Type.Property,Instance
    Property,Instance
    Namespace+Type.Property,Instance

    Type.Property
    Property
    Namespace+Type.Property

     */
    public class SettingNameByUsageGenerator : ISettingNameGenerator
    {
        private static readonly IEnumerable<Func<SettingName, SettingName>> SettingNameFactories = new Func<SettingName, SettingName>[]
        {
            source => new SettingName(source.Member) {Type = source.Type, Instance = source.Instance},
            source => new SettingName(source.Member) {Instance = source.Instance},
            source => new SettingName(source.Member) {Namespace = source.Namespace, Type = source.Type, Instance = source.Instance}
        };

        public IEnumerable<SettingName> GenerateSettingNames(SoftString settingName)
        {
            if (settingName == null) throw new ArgumentNullException(nameof(settingName));

            var localSettingName = SettingName.Parse(settingName.ToString());
            return
                Enumerable.Concat(
                        GenerateSettingNamesWithInstance(localSettingName),
                        GenerateSettingNamesWithoutInstance(localSettingName)
                    )
                    .Distinct();
        }

        private static IEnumerable<SettingName> GenerateSettingNamesWithInstance(SettingName settingName)
        {
            return
                settingName.Instance.IsNullOrEmpty()
                    ? Enumerable.Empty<SettingName>()
                    : SettingNameFactories.Select(factory => factory(settingName));
        }

        private static IEnumerable<SettingName> GenerateSettingNamesWithoutInstance(SettingName settingName)
        {
            var settingNameWithoutInstance = new SettingName(settingName.Member)
            {
                Namespace = settingName.Namespace,
                Type = settingName.Type
            };

            return SettingNameFactories.Select(factory => factory(settingNameWithoutInstance));
        }
    }

//    public class FourPartString : IReadOnlyList<string>
//    {
//        private readonly IReadOnlyList<string> _items;
//
//        public FourPartString(IList<string> items)
//        {
//            _items = new List<string>(items);
//        }
//        
//        public int Count => 4;
//
//        public IEnumerator<string> GetEnumerator() => _items.GetEnumerator();
//
//        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
//
//
//        public string this[int index] => throw new NotImplementedException();
//    }

//    public class SettingNameByUsageGenerator2 //: ISettingNameGenerator
//    {
//        public const int PartCount = 4;
//        
//        private static class Index
//        {
//            public const int Namespace = 0;
//            public const int Type = 1;
//            public const int Member = 2;
//            public const int Instance = 3;
//        }
//
//        private static readonly IEnumerable<Func<IList<string>, IList<string>>> Factories = new Func<IList<string>, IList<string>>[]
//        {
//            source => new[] {source[Index.Type], source[Index.Member]},
//            source => new[] {source[Index.Member]},
//            source => new[] {source[Index.Namespace], source[Index.Type], source[Index.Member]},            
//        };
//
//        public IEnumerable<SettingName> GenerateSettingNames(IList<string> source)
//        {            
//            if (source == null) throw new ArgumentNullException(nameof(source));
//            if (source.Count != PartCount) throw new ArgumentException(nameof(source));
//
//            if (source.Last() is null)
//            {
//                
//            }
//            
//            var localSettingName = SettingName.Parse(settingName.ToString());
//            return
//                Enumerable.Concat(
//                        GenerateSettingNamesWithInstance(localSettingName),
//                        GenerateSettingNamesWithoutInstance(localSettingName)
//                    )
//                    .Distinct();
//        }
//
//        private static IEnumerable<IList<string>> GenerateSettingNamesWithInstance(IList<string> source)
//        {
//            return Factories.Select(factory => factory(source));
//        }
//
//        private static IEnumerable<SettingName> GenerateSettingNamesWithoutInstance(SettingName settingName)
//        {
//            var settingNameWithoutInstance = new SettingName(settingName.Member)
//            {
//                Namespace = settingName.Namespace,
//                Type = settingName.Type
//            };
//
//            return Factories.Select(factory => factory(settingNameWithoutInstance));
//        }
//    }
}