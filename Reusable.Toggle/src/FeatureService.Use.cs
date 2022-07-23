using System;
using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Wiretap;
using Reusable.Wiretap.Extensions;

namespace Reusable.Toggle;

public static partial class FeatureServiceExtensions
{
    public static async Task<T?> Use<T>(this IFeatureService features, IFeatureIdentifier id, Func<Task<T?>> onEnabled, Func<Task<T?>>? onDisabled = default, object? parameter = default)
    {
        var telemetryId = Feature.Telemetry.DefaultId;
        
        // If there is the meta-feature telemetry and it's enabled then get its logger.
        var logger =
            features.TryGet(telemetryId, out var feature) && feature is Feature.Telemetry telemetry && features.IsEnabled(telemetryId)
                ? telemetry.Logger
                : Logger.Empty.Instance;

        using var scope = logger.BeginUnitOfWork("UseFeature");
        logger.Log(Telemetry.Collect.Application().Metadata(new { feature = feature.Id, policy = feature.Policy }));

        // Not catching exceptions because the caller should handle them.
        var exception = default(Exception);
        try
        {
            onDisabled ??= () => default(T).ToTask();

            return
                features.IsEnabled(id, parameter)
                    ? await onEnabled().ConfigureAwait(false)
                    : await onDisabled().ConfigureAwait(false);
        }
        catch (Exception inner)
        {
            scope.SetException(inner);
            throw DynamicException.Create("FeatureUsage", $"Could not use '{id}'. See the inner exception for details.", inner);
        }
        finally
        {
            features.Usage.AfterUse(new FeatureUsageContext(features, id, parameter, exception));
        }
    }


    public static async Task Use(this IFeatureService features, IFeatureIdentifier id, Func<Task> onEnabled, Func<Task>? onDisabled = default, object? parameter = default)
    {
        onDisabled ??= () => Task.CompletedTask;

        await features.Use<object>
        (
            id,
            async () =>
            {
                await onEnabled();
                return Task.CompletedTask;
            },
            async () =>
            {
                await onDisabled();
                return Task.CompletedTask;
            },
            parameter
        );
    }

    public static T? Use<T>(this IFeatureService features, IFeatureIdentifier id, Func<T?> onEnabled, Func<T?>? onDisabled = default, object? parameter = default)
    {
        onDisabled ??= () => default;

        return
            features
                .Use
                (
                    id,
                    () => onEnabled().ToTask(),
                    () => onDisabled().ToTask(),
                    parameter
                )
                .GetAwaiter()
                .GetResult();
    }

    public static void Use(this IFeatureService features, IFeatureIdentifier id, Action onEnabled, Action? onDisabled = default, object? parameter = default)
    {
        onDisabled ??= () => { };

        features
            .Use
            (
                id,
                () =>
                {
                    onEnabled();
                    return default(object?).ToTask();
                },
                () =>
                {
                    onDisabled();
                    return default(object?).ToTask();
                },
                parameter
            )
            .GetAwaiter()
            .GetResult();
    }

    public static T? Use<T>(this IFeatureService features, IFeatureIdentifier id, T? onEnabled, T? onDisabled = default, object? parameter = default)
    {
        return features.Use(id, () => onEnabled, () => onDisabled, parameter);
    }
}