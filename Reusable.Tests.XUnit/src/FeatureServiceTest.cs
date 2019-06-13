using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Custom;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Org.BouncyCastle.Bcpg.Sig;
using Reusable.Collections;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Foggle;
using Reusable.IOnymous;
using Reusable.OmniLog;
using Reusable.Tests.XUnit.Features;
using Xunit;

namespace Reusable.Tests.XUnit
{
    public static class Tags
    {
        public const string Database = nameof(Database);
        public const string SaveChanges = nameof(SaveChanges);
    }

    public class FeatureServiceTest
    {
        [Fact]
        public void Can_create_key_from_type_and_member()
        {
            Assert.Equal("Demo.Greeting", From<IDemo>.Select(x => x.Greeting));
        }

        [Fact]
        public void Can_configure_features_by_tags()
        {
            var features = new FeatureService
            (
                logger: Logger<FeatureService>.Null,
                defaultOptions: FeatureOption.Enable | FeatureOption.Warn | FeatureOption.Telemetry
            );

            var names =
                ImmutableList<Selector>
                    .Empty
                    .AddFrom<IDemo>()
                    .AddFrom<IDatabase>()
                    .Where<TagAttribute>("io")
                    .Select(SelectorFormatters.Plain);

            features.Configure(names, o => o ^ FeatureOption.Enable);

            var bodyCounter = 0;
            var otherCounter = 0;
            features.Execute(From<IDemo>.Select(x => x.Greeting), () => bodyCounter++, () => otherCounter++);
            features.Execute(From<IDemo>.Select(x => x.ReadFile), () => bodyCounter++, () => otherCounter++);
            features.Execute(From<IDatabase>.Select(x => x.Commit), () => bodyCounter++, () => otherCounter++);

            Assert.Equal(1, bodyCounter);
            Assert.Equal(2, otherCounter);
        }
    }

    public class FeatureServiceDemo
    {
        private readonly FeatureService _features = new FeatureService
        (
            Logger<FeatureService>.Null,
            defaultOptions: FeatureOption.Enable | FeatureOption.Warn | FeatureOption.Telemetry
        );

        public async Task Start()
        {
            SayHallo();

            //_features.Configure(nameof(SayHallo), o => o.Reset(FeatureOption.Enable));
            //_features.Configure(Use<IDemo>.Namespace, x => x.Greeting, o => o ^ Enabled);
            _features.Configure(From<IDemo>.Select(x => x.Greeting).Index("Morning"), o => o.Reset(FeatureOption.Enable));

            SayHallo();
        }

        private void SayHallo()
        {
            _features.Execute(nameof(SayHallo), () => Console.WriteLine("Hallo"), () => Console.WriteLine("You've disabled it!"));
        }
    }

    /*
     Example settings:
     
     {
        "Service.Feature1": "Enabled, LogWhenEnabled, LogWhenDisabled"
        "Service.Feature2": {
            "Options": "Enabled, LogWhenEnabled, LogWhenDisabled",
        }
     }
     
     */

    namespace Features
    {
        [UseType, UseMember]
        [TrimStart("I")]
        public interface IDemo : INamespace
        {
            object Greeting { get; }

            [Tag("io")]
            object ReadFile { get; }
        }

        [UseType, UseMember]
        [TrimStart("I")]
        public interface IDatabase : INamespace
        {
            [Tag("io")]
            object Commit { get; }
        }
    }


    //    public class UriKey<T> : Key<T, UriString>
//    {
//        public override UriString Value => throw new NotImplementedException();
//    }
}