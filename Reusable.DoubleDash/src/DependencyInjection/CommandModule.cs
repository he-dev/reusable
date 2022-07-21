using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Reusable.DoubleDash.Annotations;
using Reusable.Essentials;
using Reusable.Utilities.Autofac;

namespace Reusable.DoubleDash.DependencyInjection;

[PublicAPI]
public class CommandModule : Autofac.Module
{
    private readonly Action<ICommandRegistrationBuilder> _build;

    public CommandModule(Action<ICommandRegistrationBuilder> build) => _build = build;

    protected override void Load(ContainerBuilder builder)
    {
        builder
            .RegisterType<CommandLineTokenizer>()
            .As<ICommandLineTokenizer>();

        builder
            .RegisterType<CommandLineParser>().PropertiesAutowired()
            .As<ICommandLineParser>();

        // builder
        //     .RegisterType<CommandFactory>()
        //     .SingleInstance()
        //     .As<ICommandFactory>();

        builder
            .RegisterType<CommandParameterBinder>()
            .As<ICommandParameterBinder>();

        builder
            .RegisterType<CommandExecutor>()
            .As<ICommandExecutor>()
            .PropertiesAutowired(new AttributeSelector<ServiceAttribute>());

        var crb = new CommandRegistrationBuilder { Builder = builder }.Also(_build);

        builder
            .RegisterInstance(crb.ToList())
            .As<IEnumerable<CommandInfo>>();

        // builder
        //     .RegisterSource(new TypeListSource());
    }
}